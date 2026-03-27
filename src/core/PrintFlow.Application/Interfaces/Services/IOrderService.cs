using PrintFlow.Application.DTOs.Admin;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Orders;

namespace PrintFlow.Application.Interfaces.Services;

public interface IOrderService
{
    // ── Customer ──
    Task<ApiResult<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request);
    Task<ApiResult<List<OrderListDto>>> GetMyOrdersAsync(Guid userId);
    Task<ApiResult<OrderDetailDto>> GetOrderByIdAsync(Guid userId, Guid orderId);

    // ── Admin ──
    Task<ApiResult<PagedResponse<OrderListDto>>> GetAllOrdersAsync(
        int pageNumber, int pageSize, string? status = null,
        DateTime? fromDate = null, DateTime? toDate = null, string? searchTerm = null);
    Task<ApiResult<OrderDetailDto>> GetOrderDetailAdminAsync(Guid orderId);
    Task<ApiResult<OrderDto>> UpdateOrderStatusAsync(Guid orderId, Guid adminUserId, UpdateOrderStatusRequest request);
    Task<ApiResult<bool>> ApproveOfflinePaymentAsync(Guid orderId, Guid adminUserId);
    Task<ApiResult<bool>> CancelOrderAsync(Guid orderId, Guid adminUserId);

    // ── Dashboard ──
    Task<ApiResult<DashboardDto>> GetDashboardAsync();
}