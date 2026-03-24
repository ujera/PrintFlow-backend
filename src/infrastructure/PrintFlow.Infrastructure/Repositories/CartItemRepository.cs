using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Domain.Entities;
using PrintFlow.Persistence.Context;

namespace PrintFlow.Infrastructure.Repositories;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(PrintFlowDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CartItem>> GetByUserIdWithProductAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
                .ThenInclude(p => p.PricingTiers)
            .Include(c => c.Product)
                .ThenInclude(p => p.Options)
            .ToListAsync(cancellationToken);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _dbSet
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(items);
    }
}