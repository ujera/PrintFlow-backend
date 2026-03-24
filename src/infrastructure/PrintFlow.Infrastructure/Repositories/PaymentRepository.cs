using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Domain.Entities;
using PrintFlow.Persistence.Context;

namespace PrintFlow.Infrastructure.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(PrintFlowDbContext context) : base(context) { }

    public async Task<Payment?> GetByStripePaymentIdAsync(string stripePaymentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.StripePaymentId == stripePaymentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}