using Microsoft.EntityFrameworkCore.Storage;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Domain.Entities;
using PrintFlow.Persistence.Context;

namespace PrintFlow.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PrintFlowDbContext _context;
        private IDbContextTransaction? _transaction;

        private IProductCategoryRepository? _productCategories;
        private IProductRepository? _products;
        private IGenericRepository<ProductOption>? _productOptions;
        private IGenericRepository<PricingTier>? _pricingTiers;
        private IOrderRepository? _orders;
        private IGenericRepository<OrderItem>? _orderItems;
        private IPaymentRepository? _payments;
        private IGenericRepository<OrderStatusHistory>? _orderStatusHistories;
        private ICartItemRepository? _cartItems;
        private INotificationRepository? _notifications;
        public UnitOfWork(PrintFlowDbContext context)
        {
            _context = context;
        }

        // ── Repository accessors ──

        public IProductCategoryRepository ProductCategories =>
            _productCategories ??= new ProductCategoryRepository(_context);

        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        public IGenericRepository<ProductOption> ProductOptions =>
            _productOptions ??= new GenericRepository<ProductOption>(_context);

        public IGenericRepository<PricingTier> PricingTiers =>
            _pricingTiers ??= new GenericRepository<PricingTier>(_context);

        public IOrderRepository Orders =>
            _orders ??= new OrderRepository(_context);

        public IGenericRepository<OrderItem> OrderItems =>
            _orderItems ??= new GenericRepository<OrderItem>(_context);

        public IPaymentRepository Payments =>
            _payments ??= new PaymentRepository(_context);

        public IGenericRepository<OrderStatusHistory> OrderStatusHistories =>
            _orderStatusHistories ??= new GenericRepository<OrderStatusHistory>(_context);

        public ICartItemRepository CartItems =>
            _cartItems ??= new CartItemRepository(_context);

        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        // ── Persistence ──

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        
        // ── Dispose ──

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
