using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Domain.Entities;
using PrintFlow.Persistence.Context;

namespace PrintFlow.Infrastructure.Repositories;

public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(PrintFlowDbContext context) : base(context) { }

    public async Task<ProductCategory?> GetBySlugAsync(string slug,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(c => c.Products.Where(p => p.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllActiveOrderedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}