using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Interfaces.Repositories;

public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
{
    Task<ProductCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductCategory>> GetAllActiveOrderedAsync(CancellationToken cancellationToken = default);
}