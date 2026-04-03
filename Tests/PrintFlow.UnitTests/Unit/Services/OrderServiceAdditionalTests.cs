using AutoMapper;
using MassTransit;
using Moq;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Mappings;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.UnitTests.Unit.Services;

public class OrderServiceAdditionalTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly Mock<IPaymentProcessingService> _paymentService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly OrderService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _adminId = Guid.NewGuid();

    public OrderServiceAdditionalTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _paymentService = new Mock<IPaymentProcessingService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _service = new OrderService(_unitOfWork.Object, _mapper, _paymentService.Object, _publishEndpoint.Object);

        _unitOfWork.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.CommitTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.RollbackTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    // ── GetMyOrdersAsync ──

    [Fact]
    public async Task GetMyOrders_ReturnsUserOrders()
    {
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, Status = OrderStatus.Paid, TotalAmount = 50, Items = new List<OrderItem>(), User = new User { Name = "Test", Email = "test@test.com" } },
            new() { Id = Guid.NewGuid(), UserId = _userId, Status = OrderStatus.Completed, TotalAmount = 100, Items = new List<OrderItem>(), User = new User { Name = "Test", Email = "test@test.com" } }
        };

        _unitOfWork.Setup(u => u.Orders.GetByUserIdAsync(_userId, default)).ReturnsAsync(orders);

        var result = await _service.GetMyOrdersAsync(_userId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetMyOrders_NoOrders_ReturnsEmpty()
    {
        _unitOfWork.Setup(u => u.Orders.GetByUserIdAsync(_userId, default))
            .ReturnsAsync(new List<Order>());

        var result = await _service.GetMyOrdersAsync(_userId);

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    // ── GetOrderDetailAdminAsync ──

    [Fact]
    public async Task GetOrderDetailAdmin_Found_ReturnsDetail()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            UserId = _userId,
            Status = OrderStatus.Paid,
            TotalAmount = 75,
            Items = new List<OrderItem>(),
            Payments = new List<Payment>(),
            StatusHistory = new List<OrderStatusHistory>(),
            User = new User { Name = "Test", Email = "test@test.com" }
        };

        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(orderId, default)).ReturnsAsync(order);

        var result = await _service.GetOrderDetailAdminAsync(orderId);

        Assert.True(result.Success);
        Assert.Equal(orderId, result.Data!.Id);
    }

    [Fact]
    public async Task GetOrderDetailAdmin_NotFound_Throws()
    {
        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetOrderDetailAdminAsync(Guid.NewGuid()));
    }

    // ── GetAllOrdersAsync ──

    [Fact]
    public async Task GetAllOrders_ReturnsPaged()
    {
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), Status = OrderStatus.Paid, TotalAmount = 50, Items = new List<OrderItem>(), User = new User { Name = "A", Email = "a@test.com" } }
        };

        _unitOfWork.Setup(u => u.Orders.GetFilteredPagedAsync(1, 20, null, null, null, null, default))
            .ReturnsAsync((orders as IReadOnlyList<Order>, 1));

        var result = await _service.GetAllOrdersAsync(1, 20, null, null, null, null);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalCount);
    }

    [Fact]
    public async Task GetAllOrders_WithStatusFilter_PassesFilter()
    {
        _unitOfWork.Setup(u => u.Orders.GetFilteredPagedAsync(1, 10, OrderStatus.Paid, null, null, null, default))
            .ReturnsAsync((new List<Order>() as IReadOnlyList<Order>, 0));

        var result = await _service.GetAllOrdersAsync(1, 10, "Paid", null, null, null);

        Assert.True(result.Success);
        _unitOfWork.Verify(u => u.Orders.GetFilteredPagedAsync(1, 10, OrderStatus.Paid, null, null, null, default), Times.Once);
    }

    // ── GetDashboardAsync ──

    [Fact]
    public async Task GetDashboard_ReturnsAllCounts()
    {
        _unitOfWork.Setup(u => u.Orders.CountByStatusAsync(OrderStatus.AwaitingPayment, default)).ReturnsAsync(3);
        _unitOfWork.Setup(u => u.Orders.CountByStatusAsync(OrderStatus.InProduction, default)).ReturnsAsync(5);
        _unitOfWork.Setup(u => u.Orders.CountByStatusAsync(OrderStatus.ReadyForPickup, default)).ReturnsAsync(2);
        _unitOfWork.Setup(u => u.Orders.CountCompletedTodayAsync(default)).ReturnsAsync(8);
        _unitOfWork.Setup(u => u.Orders.GetRecentAsync(10, default))
            .ReturnsAsync(new List<Order>());

        var result = await _service.GetDashboardAsync();

        Assert.True(result.Success);
        Assert.Equal(3, result.Data!.AwaitingPaymentCount);
        Assert.Equal(5, result.Data.InProductionCount);
        Assert.Equal(2, result.Data.ReadyForPickupCount);
        Assert.Equal(8, result.Data.CompletedTodayCount);
    }

    // ── RefundOrderAsync ──

    [Fact]
    public async Task RefundOrder_NoSuccessfulPayment_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, PaymentStatus = PaymentStatus.Pending };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.RefundOrderAsync(orderId, _adminId));
    }

    [Fact]
    public async Task RefundOrder_NoStripePayment_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, PaymentStatus = PaymentStatus.Success };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.Payments.GetByOrderIdAsync(orderId, default))
            .ReturnsAsync(new List<Payment>
            {
                new() { Method = PaymentMethod.BankTransfer, Status = PaymentStatus.Success }
            });

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.RefundOrderAsync(orderId, _adminId));
    }

    [Fact]
    public async Task RefundOrder_SuccessfulRefund_CancelsOrder()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = _userId, PaymentStatus = PaymentStatus.Success, TotalAmount = 100, Status = OrderStatus.Paid };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.Payments.GetByOrderIdAsync(orderId, default))
            .ReturnsAsync(new List<Payment>
            {
                new() { Method = PaymentMethod.Card, Status = PaymentStatus.Success, StripePaymentId = "pi_123" }
            });
        _paymentService.Setup(p => p.RefundPaymentAsync("pi_123", 100))
            .ReturnsAsync(new RefundResult { Success = true, RefundId = "re_123", Status = "succeeded" });
        _unitOfWork.Setup(u => u.Payments.AddAsync(It.IsAny<Payment>(), default))
            .ReturnsAsync((Payment p, CancellationToken _) => p);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        var result = await _service.RefundOrderAsync(orderId, _adminId);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(PaymentStatus.Refunded, order.PaymentStatus);
    }

    // ── UpdateStatus publishes events ──

    [Fact]
    public async Task UpdateStatus_ToCompleted_PublishesCompletedEvent()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.ReadyForPickup, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.UpdateOrderStatusAsync(orderId, _adminId, new UpdateOrderStatusRequest { Status = "Completed" });

        _publishEndpoint.Verify(p => p.Publish(
            It.IsAny<Application.Messages.OrderCompletedEvent>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_NotCompleted_DoesNotPublishCompletedEvent()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.UpdateOrderStatusAsync(orderId, _adminId, new UpdateOrderStatusRequest { Status = "InProduction" });

        _publishEndpoint.Verify(p => p.Publish(
            It.IsAny<Application.Messages.OrderCompletedEvent>(), default), Times.Never);
    }

    // ── Cancel publishes event ──

    [Fact]
    public async Task CancelOrder_PublishesStatusChangedEvent()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.CancelOrderAsync(orderId, _adminId);

        _publishEndpoint.Verify(p => p.Publish(
            It.IsAny<Application.Messages.OrderStatusChangedEvent>(), default), Times.Once);
    }
}