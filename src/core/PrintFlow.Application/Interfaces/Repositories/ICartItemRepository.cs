using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Interfaces.Repositories;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
    Task<IReadOnlyList<CartItem>> GetByUserIdWithProductAsync(Guid userId,
        CancellationToken cancellationToken = default);

    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}