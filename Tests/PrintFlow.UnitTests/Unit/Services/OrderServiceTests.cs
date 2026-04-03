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

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly Mock<IPaymentProcessingService> _paymentService;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly OrderService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _adminId = Guid.NewGuid();

    public OrderServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _paymentService = new Mock<IPaymentProcessingService>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new OrderService(_unitOfWork.Object, _mapper, _paymentService.Object, _publishEndpoint.Object);

        // Default transaction setup
        _unitOfWork.Setup(u => u.BeginTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.CommitTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.RollbackTransactionAsync(default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
    }

    // ══════════════════════════════════════════
    //  CREATE ORDER
    // ══════════════════════════════════════════

    [Fact]
    public async Task CreateOrder_EmptyCart_ThrowsBadRequest()
    {
        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(new List<CartItem>());

        var request = new CreateOrderRequest { PaymentMethod = "Card" };

        await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateOrderAsync(_userId, request));
    }

    [Fact]
    public async Task CreateOrder_BankTransfer_StatusIsAwaitingPayment()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Card",
            BasePrice = 10m,
            PricingTiers = new List<PricingTier>()
        };

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product.Id, Product = product, Quantity = 5 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(cartItems);
        _unitOfWork.Setup(u => u.Orders.AddAsync(It.IsAny<Order>(), default))
            .ReturnsAsync((Order o, CancellationToken _) => o);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);
        _unitOfWork.Setup(u => u.CartItems.ClearCartAsync(_userId, default))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Guid id, CancellationToken _) => new Order
            {
                Id = id,
                UserId = _userId,
                Status = OrderStatus.AwaitingPayment,
                PaymentMethod = PaymentMethod.BankTransfer,
                TotalAmount = 50m,
                Items = new List<OrderItem>()
            });

        var request = new CreateOrderRequest { PaymentMethod = "BankTransfer" };
        var result = await _service.CreateOrderAsync(_userId, request);

        Assert.True(result.Success);
        Assert.Equal("AwaitingPayment", result.Data!.Status);
    }

    [Fact]
    public async Task CreateOrder_Card_StatusIsCreated()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Card",
            BasePrice = 10m,
            PricingTiers = new List<PricingTier>()
        };

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product.Id, Product = product, Quantity = 2 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(cartItems);
        _unitOfWork.Setup(u => u.Orders.AddAsync(It.IsAny<Order>(), default))
            .ReturnsAsync((Order o, CancellationToken _) => o);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);
        _unitOfWork.Setup(u => u.CartItems.ClearCartAsync(_userId, default))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Guid id, CancellationToken _) => new Order
            {
                Id = id,
                UserId = _userId,
                Status = OrderStatus.Created,
                PaymentMethod = PaymentMethod.Card,
                TotalAmount = 20m,
                Items = new List<OrderItem>()
            });

        var request = new CreateOrderRequest { PaymentMethod = "Card" };
        var result = await _service.CreateOrderAsync(_userId, request);

        Assert.True(result.Success);
        Assert.Equal("Created", result.Data!.Status);
    }

    [Fact]
    public async Task CreateOrder_CalculatesTotalFromPricingTiers()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Sticker",
            BasePrice = 0.10m,
            PricingTiers = new List<PricingTier>
            {
                new() { MinQuantity = 200, MaxQuantity = 499, UnitPrice = 0.07m }
            }
        };

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product.Id, Product = product, Quantity = 300 }
        };

        Order? capturedOrder = null;
        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(cartItems);
        _unitOfWork.Setup(u => u.Orders.AddAsync(It.IsAny<Order>(), default))
            .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
            .ReturnsAsync((Order o, CancellationToken _) => o);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);
        _unitOfWork.Setup(u => u.CartItems.ClearCartAsync(_userId, default))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new Order { Items = new List<OrderItem>(), Status = OrderStatus.Created, TotalAmount = 21m });

        await _service.CreateOrderAsync(_userId, new CreateOrderRequest { PaymentMethod = "Card" });

        Assert.NotNull(capturedOrder);
        // 300 × $0.07 = $21
        Assert.Equal(21m, capturedOrder!.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_ClearsCart()
    {
        var product = new Product { Id = Guid.NewGuid(), BasePrice = 5m, PricingTiers = new List<PricingTier>() };
        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product.Id, Product = product, Quantity = 1 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default)).ReturnsAsync(cartItems);
        _unitOfWork.Setup(u => u.Orders.AddAsync(It.IsAny<Order>(), default)).ReturnsAsync((Order o, CancellationToken _) => o);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default)).ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);
        _unitOfWork.Setup(u => u.CartItems.ClearCartAsync(_userId, default)).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new Order { Items = new List<OrderItem>(), Status = OrderStatus.Created });

        await _service.CreateOrderAsync(_userId, new CreateOrderRequest { PaymentMethod = "Card" });

        _unitOfWork.Verify(u => u.CartItems.ClearCartAsync(_userId, default), Times.Once);
    }

    // ══════════════════════════════════════════
    //  UPDATE STATUS
    // ══════════════════════════════════════════

    [Fact]
    public async Task UpdateOrderStatus_ValidTransition_UpdatesStatus()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        var request = new UpdateOrderStatusRequest { Status = "InProduction", Notes = "Started" };
        var result = await _service.UpdateOrderStatusAsync(orderId, _adminId, request);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.InProduction, order.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidTransition_ThrowsInvalidOrderState()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Completed };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        var request = new UpdateOrderStatusRequest { Status = "InProduction" };

        await Assert.ThrowsAsync<InvalidOrderStateException>(() =>
            _service.UpdateOrderStatusAsync(orderId, _adminId, request));
    }

    [Fact]
    public async Task UpdateOrderStatus_NotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateOrderStatusAsync(orderId, _adminId, new UpdateOrderStatusRequest { Status = "Paid" }));
    }

    [Fact]
    public async Task UpdateOrderStatus_LogsStatusHistory()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.UpdateOrderStatusAsync(orderId, _adminId, new UpdateOrderStatusRequest { Status = "InProduction" });

        _unitOfWork.Verify(u => u.OrderStatusHistories.AddAsync(
            It.Is<OrderStatusHistory>(h => h.OldStatus == OrderStatus.Paid && h.NewStatus == OrderStatus.InProduction),
            default), Times.Once);
    }

    // ══════════════════════════════════════════
    //  APPROVE OFFLINE PAYMENT
    // ══════════════════════════════════════════

    [Fact]
    public async Task ApprovePayment_AwaitingPayment_SetsToPaid()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.AwaitingPayment, TotalAmount = 100m, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.Payments.AddAsync(It.IsAny<Payment>(), default))
            .ReturnsAsync((Payment p, CancellationToken _) => p);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        var result = await _service.ApproveOfflinePaymentAsync(orderId, _adminId);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(PaymentStatus.Success, order.PaymentStatus);
    }

    [Fact]
    public async Task ApprovePayment_NotAwaitingPayment_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.ApproveOfflinePaymentAsync(orderId, _adminId));
    }

    [Fact]
    public async Task ApprovePayment_CreatesPaymentRecord()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.AwaitingPayment, TotalAmount = 50m, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.Payments.AddAsync(It.IsAny<Payment>(), default))
            .ReturnsAsync((Payment p, CancellationToken _) => p);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.ApproveOfflinePaymentAsync(orderId, _adminId);

        _unitOfWork.Verify(u => u.Payments.AddAsync(
            It.Is<Payment>(p => p.Method == PaymentMethod.BankTransfer && p.Amount == 50m && p.Status == PaymentStatus.Success),
            default), Times.Once);
    }

    // ══════════════════════════════════════════
    //  CANCEL ORDER
    // ══════════════════════════════════════════

    [Fact]
    public async Task CancelOrder_ValidStatus_Cancels()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Paid, UserId = _userId };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        var result = await _service.CancelOrderAsync(orderId, _adminId);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelOrder_CompletedOrder_ThrowsInvalidState()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.Completed };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOrderStateException>(() =>
            _service.CancelOrderAsync(orderId, _adminId));
    }

    // ══════════════════════════════════════════
    //  GET ORDER — AUTHORIZATION
    // ══════════════════════════════════════════

    [Fact]
    public async Task GetOrderById_DifferentUser_ThrowsForbidden()
    {
        var orderId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _unitOfWork.Setup(u => u.Orders.GetWithDetailsAsync(orderId, default))
            .ReturnsAsync(new Order
            {
                Id = orderId,
                UserId = otherUserId,
                Items = new List<OrderItem>(),
                Payments = new List<Payment>(),
                StatusHistory = new List<OrderStatusHistory>(),
                User = new User { Name = "Other", Email = "other@test.com" }
            });

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetOrderByIdAsync(_userId, orderId));
    }

    // ══════════════════════════════════════════
    //  INITIATE PAYMENT
    // ══════════════════════════════════════════

    [Fact]
    public async Task InitiatePayment_NotCardMethod_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = _userId, PaymentMethod = PaymentMethod.BankTransfer, Status = OrderStatus.Created };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.InitiatePaymentAsync(_userId, orderId));
    }

    [Fact]
    public async Task InitiatePayment_AlreadyPaid_ThrowsBadRequest()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = _userId, PaymentMethod = PaymentMethod.Card, Status = OrderStatus.Paid };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            _service.InitiatePaymentAsync(_userId, orderId));
    }

    [Fact]
    public async Task InitiatePayment_DifferentUser_ThrowsForbidden()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = Guid.NewGuid(), PaymentMethod = PaymentMethod.Card, Status = OrderStatus.Created };

        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.InitiatePaymentAsync(_userId, orderId));
    }

    // ══════════════════════════════════════════
    //  WEBHOOK HANDLERS
    // ══════════════════════════════════════════

    [Fact]
    public async Task HandlePaymentSucceeded_UpdatesOrderAndPayment()
    {
        var orderId = Guid.NewGuid();
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, StripePaymentId = "pi_test", Status = PaymentStatus.Pending };
        var order = new Order { Id = orderId, Status = OrderStatus.Created, UserId = _userId };

        _unitOfWork.Setup(u => u.Payments.GetByStripePaymentIdAsync("pi_test", default)).ReturnsAsync(payment);
        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.HandlePaymentSucceededAsync("pi_test");

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(PaymentStatus.Success, order.PaymentStatus);
        Assert.Equal(PaymentStatus.Success, payment.Status);
        Assert.NotNull(payment.ProcessedAt);
    }

    [Fact]
    public async Task HandlePaymentFailed_UpdatesOrderAndPayment()
    {
        var orderId = Guid.NewGuid();
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, StripePaymentId = "pi_fail", Status = PaymentStatus.Pending };
        var order = new Order { Id = orderId, Status = OrderStatus.Created, UserId = _userId };

        _unitOfWork.Setup(u => u.Payments.GetByStripePaymentIdAsync("pi_fail", default)).ReturnsAsync(payment);
        _unitOfWork.Setup(u => u.Orders.GetByIdAsync(orderId, default)).ReturnsAsync(order);
        _unitOfWork.Setup(u => u.OrderStatusHistories.AddAsync(It.IsAny<OrderStatusHistory>(), default))
            .ReturnsAsync((OrderStatusHistory h, CancellationToken _) => h);

        await _service.HandlePaymentFailedAsync("pi_fail");

        Assert.Equal(OrderStatus.PaymentFailed, order.Status);
        Assert.Equal(PaymentStatus.Failed, order.PaymentStatus);
        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    [Fact]
    public async Task HandlePaymentSucceeded_UnknownPaymentId_DoesNothing()
    {
        _unitOfWork.Setup(u => u.Payments.GetByStripePaymentIdAsync("pi_unknown", default))
            .ReturnsAsync((Payment?)null);

        await _service.HandlePaymentSucceededAsync("pi_unknown");

        _unitOfWork.Verify(u => u.Orders.Update(It.IsAny<Order>()), Times.Never);
    }
}