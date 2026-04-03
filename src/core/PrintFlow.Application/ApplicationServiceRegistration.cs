using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Services;
using PrintFlow.Infrastructure.Services;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;


namespace PrintFlow.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPaymentProcessingService, StripePaymentService>();
        


        // External services
        services.AddScoped<IPaymentProcessingService, StripePaymentService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // RabbitMQ via MassTransit (publisher only — no consumers in API)
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