using AutoMapper;
using PrintFlow.Application.DTOs.Admin;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // ── Customer ──

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

            // Build order items from cart
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

            // Set initial status based on payment method
            if (paymentMethod == PaymentMethod.BankTransfer)
            {
                order.Status = OrderStatus.AwaitingPayment;
            }

            await _unitOfWork.Orders.AddAsync(order);

            // Log initial status
            await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                OldStatus = OrderStatus.Created,
                NewStatus = order.Status,
                ChangedByUserId = userId,
                Notes = "Order created"
            });

            // Clear cart
            await _unitOfWork.CartItems.ClearCartAsync(userId);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var created = await _unitOfWork.Orders.GetWithDetailsAsync(order.Id);
            return ApiResult<OrderDto>.Ok(_mapper.Map<OrderDto>(created!), "Order created.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
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

    // ── Admin ──

    public async Task<ApiResult<PagedResponse<OrderListDto>>> GetAllOrdersAsync(
        int pageNumber, int pageSize, string? status, DateTime? fromDate,
        DateTime? toDate, string? searchTerm)
    {
        OrderStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
        {
            statusFilter = parsed;
        }

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

        return ApiResult<bool>.Ok(true, "Order cancelled.");
    }

    // ── Dashboard ──

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