using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Services;
using PrintFlow.Infrastructure.Repositories;
using PrintFlow.Infrastructure.Services;

namespace PrintFlow.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // External services
        services.AddScoped<IPaymentProcessingService, StripePaymentService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // RabbitMQ via MassTransit (publisher only)
        services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });
            });
        });

        return services;
    }
}