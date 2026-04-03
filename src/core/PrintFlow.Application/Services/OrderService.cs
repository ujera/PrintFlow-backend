using AutoMapper;
using MassTransit;
using PrintFlow.Application.DTOs.Admin;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Messages;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPaymentProcessingService _paymentService;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper,
        IPaymentProcessingService paymentService, IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentService = paymentService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ApiResult<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        var cartItems = await _unitOfWork.CartItems.GetByUserIdWithProductAsync(userId);
        if (cartItems.Count == 0)
            throw new BadRequestException("Cart is empty.");

        var paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod, true);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                UserId = userId,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Created,
                PaymentStatus = PaymentStatus.Pending,
                Notes = request.Notes
            };

            decimal total = 0;
            foreach (var cartItem in cartItems)
            {
                var tier = cartItem.Product?.PricingTiers?
                    .Where(t => cartItem.Quantity >= t.MinQuantity && cartItem.Quantity <= t.MaxQuantity)
                    .FirstOrDefault();

                var unitPrice = tier?.UnitPrice ?? cartItem.Product?.BasePrice ?? 0;
                var subtotal = unitPrice * cartItem.Quantity;
                total += subtotal;

                order.Items.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    Subtotal = subtotal,
                    ConfigJson = cartItem.ConfigJson,
                    UploadFileUrl = cartItem.UploadFileUrl
                });
            }

            order.TotalAmount = total;

            if (paymentMethod == PaymentMethod.BankTransfer)
                order.Status = OrderStatus.AwaitingPayment;

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = OrderStatus.Created,
                NewStatus = order.Status,
                ChangedByUserId = userId,
                Notes = "Order created"
            });

            await _unitOfWork.CartItems.ClearCartAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _publishEndpoint.Publish(new OrderCreatedEvent(order.Id));

            var created = await _unitOfWork.Orders.GetWithDetailsAsync(order.Id);
            return ApiResult<OrderDto>.Ok(_mapper.Map<OrderDto>(created!), "Order created.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApiResult<PaymentIntentDto>> InitiatePaymentAsync(Guid userId, Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.UserId != userId)
            throw new ForbiddenException();

        if (order.PaymentMethod != PaymentMethod.Card)
            throw new BadRequestException("This order does not use card payment.");

        if (order.Status != OrderStatus.Created && order.Status != OrderStatus.PaymentFailed)
            throw new BadRequestException("Payment cannot be initiated for this order status.");

        var result = await _paymentService.CreatePaymentIntentAsync(orderId, order.TotalAmount);

        await _unitOfWork.Payments.AddAsync(new Payment
        {
            OrderId = orderId,
            Method = PaymentMethod.Card,
            Status = PaymentStatus.Pending,
            StripePaymentId = result.PaymentIntentId,
            Amount = order.TotalAmount
        });
        await _unitOfWork.SaveChangesAsync();

        return ApiResult<PaymentIntentDto>.Ok(new PaymentIntentDto
        {
            ClientSecret = result.ClientSecret,
            PaymentIntentId = result.PaymentIntentId
        });
    }

    public async Task<ApiResult<List<OrderListDto>>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
        return ApiResult<List<OrderListDto>>.Ok(_mapper.Map<List<OrderListDto>>(orders));
    }

    public async Task<ApiResult<OrderDetailDto>> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.UserId != userId)
            throw new ForbiddenException();

        return ApiResult<OrderDetailDto>.Ok(_mapper.Map<OrderDetailDto>(order));
    }

    public async Task HandlePaymentSucceededAsync(string stripePaymentIntentId)
    {
        var payment = await _unitOfWork.Payments.GetByStripePaymentIdAsync(stripePaymentIntentId);
        if (payment is null) return;

        var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
        if (order is null) return;

        payment.Status = PaymentStatus.Success;
        payment.ProcessedAt = DateTime.UtcNow;

        var oldStatus = order.Status;
        order.Status = OrderStatus.Paid;
        order.PaymentStatus = PaymentStatus.Success;

        await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Paid,
            ChangedByUserId = order.UserId,
            Notes = "Payment confirmed via Stripe"
        });

        _unitOfWork.Payments.Update(payment);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _publishEndpoint.Publish(new PaymentSucceededEvent(order.Id));
    }

    public async Task HandlePaymentFailedAsync(string stripePaymentIntentId)
    {
        var payment = await _unitOfWork.Payments.GetByStripePaymentIdAsync(stripePaymentIntentId);
        if (payment is null) return;

        var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
        if (order is null) return;

        payment.Status = PaymentStatus.Failed;
        payment.ProcessedAt = DateTime.UtcNow;

        var oldStatus = order.Status;
        order.Status = OrderStatus.PaymentFailed;
        order.PaymentStatus = PaymentStatus.Failed;

        await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.PaymentFailed,
            ChangedByUserId = order.UserId,
            Notes = "Payment failed via Stripe"
        });

        _unitOfWork.Payments.Update(payment);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _publishEndpoint.Publish(new PaymentFailedEvent(order.Id));
    }

    public async Task<ApiResult<PagedResponse<OrderListDto>>> GetAllOrdersAsync(
        int pageNumber, int pageSize, string? status, DateTime? fromDate,
        DateTime? toDate, string? searchTerm)
    {
        OrderStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            statusFilter = parsed;

        var (items, totalCount) = await _unitOfWork.Orders.GetFilteredPagedAsync(
            pageNumber, pageSize, statusFilter, fromDate, toDate, searchTerm);

        return ApiResult<PagedResponse<OrderListDto>>.Ok(new PagedResponse<OrderListDto>
        {
            Items = _mapper.Map<List<OrderListDto>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResult<OrderDetailDto>> GetOrderDetailAdminAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        return ApiResult<OrderDetailDto>.Ok(_mapper.Map<OrderDetailDto>(order));
    }

    public async Task<ApiResult<OrderDto>> UpdateOrderStatusAsync(
        Guid orderId, Guid adminUserId, UpdateOrderStatusRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        var newStatus = Enum.Parse<OrderStatus>(request.Status, true);

        if (!OrderStateMachine.CanTransition(order.Status, newStatus))
            throw new InvalidOrderStateException(order.Status, newStatus);

        var oldStatus = order.Status;
        order.Status = newStatus;

        await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = adminUserId,
            Notes = request.Notes
        });

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderStatusChangedEvent(orderId, oldStatus.ToString(), newStatus.ToString()));

        if (newStatus == OrderStatus.Completed)
            await _publishEndpoint.Publish(new OrderCompletedEvent(orderId));

        return ApiResult<OrderDto>.Ok(_mapper.Map<OrderDto>(order), $"Order status updated to {newStatus}.");
    }

    public async Task<ApiResult<bool>> ApproveOfflinePaymentAsync(Guid orderId, Guid adminUserId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.Status != OrderStatus.AwaitingPayment)
            throw new BadRequestException("Order is not awaiting payment.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var oldStatus = order.Status;
            order.Status = OrderStatus.Paid;
            order.PaymentStatus = PaymentStatus.Success;

            await _unitOfWork.Payments.AddAsync(new Payment
            {
                OrderId = orderId,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Success,
                Amount = order.TotalAmount,
                ProcessedAt = DateTime.UtcNow
            });

            await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = OrderStatus.Paid,
                ChangedByUserId = adminUserId,
                Notes = "Offline payment approved by admin"
            });

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _publishEndpoint.Publish(new PaymentSucceededEvent(orderId));

            return ApiResult<bool>.Ok(true, "Payment approved.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApiResult<bool>> CancelOrderAsync(Guid orderId, Guid adminUserId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (!OrderStateMachine.CanTransition(order.Status, OrderStatus.Cancelled))
            throw new InvalidOrderStateException(order.Status, OrderStatus.Cancelled);

        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;

        await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Cancelled,
            ChangedByUserId = adminUserId,
            Notes = "Order cancelled by admin"
        });

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        await _publishEndpoint.Publish(new OrderStatusChangedEvent(orderId, oldStatus.ToString(), "Cancelled"));

        return ApiResult<bool>.Ok(true, "Order cancelled.");
    }

    public async Task<ApiResult<bool>> RefundOrderAsync(Guid orderId, Guid adminUserId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new NotFoundException("Order", orderId);

        if (order.PaymentStatus != PaymentStatus.Success)
            throw new BadRequestException("Order has no successful payment to refund.");

        var payments = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        var successfulPayment = payments.FirstOrDefault(p =>
            p.Status == PaymentStatus.Success && p.Method == PaymentMethod.Card);

        if (successfulPayment?.StripePaymentId is null)
            throw new BadRequestException("No Stripe payment found to refund.");

        var refundResult = await _paymentService.RefundPaymentAsync(
            successfulPayment.StripePaymentId, order.TotalAmount);

        if (!refundResult.Success)
            throw new PaymentException("Refund failed.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Payments.AddAsync(new Payment
            {
                OrderId = orderId,
                Method = PaymentMethod.Card,
                Status = PaymentStatus.Refunded,
                StripePaymentId = refundResult.RefundId,
                Amount = order.TotalAmount,
                ProcessedAt = DateTime.UtcNow
            });

            var oldStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            order.PaymentStatus = PaymentStatus.Refunded;

            await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = OrderStatus.Cancelled,
                ChangedByUserId = adminUserId,
                Notes = "Order refunded and cancelled"
            });

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResult<bool>.Ok(true, "Order refunded.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<ApiResult<DashboardDto>> GetDashboardAsync()
    {
        var recentOrders = await _unitOfWork.Orders.GetRecentAsync(10);

        return ApiResult<DashboardDto>.Ok(new DashboardDto
        {
            AwaitingPaymentCount = await _unitOfWork.Orders.CountByStatusAsync(OrderStatus.AwaitingPayment),
            InProductionCount = await _unitOfWork.Orders.CountByStatusAsync(OrderStatus.InProduction),
            ReadyForPickupCount = await _unitOfWork.Orders.CountByStatusAsync(OrderStatus.ReadyForPickup),
            CompletedTodayCount = await _unitOfWork.Orders.CountCompletedTodayAsync(),
            RecentOrders = _mapper.Map<List<OrderListDto>>(recentOrders)
        });
    }
}