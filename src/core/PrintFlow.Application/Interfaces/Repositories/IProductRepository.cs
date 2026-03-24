using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetActiveByCategoryIdAsync(Guid categoryId,
        CancellationToken cancellationToken = default);
}