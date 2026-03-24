using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Interfaces.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetFilteredPagedAsync(
        int pageNumber,
        int pageSize,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    // ── Dashboard queries ──
    Task<int> CountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<int> CountCompletedTodayAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}