using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Services;
using PrintFlow.Infrastructure.Services;
using System.Reflection;

namespace PrintFlow.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
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

        return services;
    }
}