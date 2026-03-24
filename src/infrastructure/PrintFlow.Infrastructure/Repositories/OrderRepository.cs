using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;
using PrintFlow.Persistence.Context;

namespace PrintFlow.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(PrintFlowDbContext context) : base(context) { }

    public async Task<Order?> GetWithDetailsAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory.OrderBy(h => h.ChangedAt))
                .ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetFilteredPagedAsync(
        int pageNumber,
        int pageSize,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _dbSet.AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(o =>
                o.User.Name.ToLower().Contains(term) ||
                o.User.Email!.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // ── Dashboard ──

    public async Task<int> CountByStatusAsync(OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(o => o.Status == status, cancellationToken);
    }

    public async Task<int> CountCompletedTodayAsync(CancellationToken cancellationToken = default)
    {
        var todayUtc = DateTime.UtcNow.Date;

        return await _dbSet.CountAsync(
            o => o.Status == OrderStatus.Completed && o.UpdatedAt >= todayUtc,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}