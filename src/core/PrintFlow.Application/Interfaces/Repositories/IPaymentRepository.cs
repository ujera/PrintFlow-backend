using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Interfaces.Repositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<Payment?> GetByStripePaymentIdAsync(string stripePaymentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId,
        CancellationToken cancellationToken = default);
}