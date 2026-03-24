using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Domain.Entities;
using PrintFlow.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(PrintFlowDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Product>> GetActiveByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().
                Where(p => p.CategoryId == categoryId && p.IsActive).
                Include(p => p.PricingTiers).
                ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().
                Where(p => p.CategoryId == categoryId).
                Include(p => p.PricingTiers).
                ToListAsync(cancellationToken);

        }

        public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().
                Include(p => p.Options).
                Include(p => p.PricingTiers.OrderBy(t => t.MinQuantity)).
                FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        }

        public Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbSet.AsNoTracking().
                Include(p => p.Category).
                Include(p => p.Options).
                Include(p => p.PricingTiers.OrderBy(t => t.MinQuantity)).
                FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}
