using PrintFlow.Application.DTOs.Orders;

namespace PrintFlow.Application.DTOs.Admin;

public class DashboardDto
{
    public int AwaitingPaymentCount { get; set; }
    public int InProductionCount { get; set; }
    public int ReadyForPickupCount { get; set; }
    public int CompletedTodayCount { get; set; }
    public List<OrderListDto> RecentOrders { get; set; } = new();
}