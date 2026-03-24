using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    // ── Repositories ──
    IProductCategoryRepository ProductCategories { get; }
    IProductRepository Products { get; }
    IGenericRepository<ProductOption> ProductOptions { get; }
    IGenericRepository<PricingTier> PricingTiers { get; }
    IOrderRepository Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IPaymentRepository Payments { get; }
    IGenericRepository<OrderStatusHistory> OrderStatusHistories { get; }
    ICartItemRepository CartItems { get; }
    INotificationRepository Notifications { get; }

    // ── Persistence ──
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}