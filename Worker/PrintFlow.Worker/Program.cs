using MassTransit;
using Microsoft.EntityFrameworkCore;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Infrastructure.Repositories;
using PrintFlow.Application.Services;
using PrintFlow.Persistence;
using PrintFlow.Persistence.Context;
using PrintFlow.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// ── Database ──
builder.Services.AddDbContext<PrintFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(ConnectionStrings.DefaultConnection)));

// ── Repositories ──
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Services ──
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ── RabbitMQ via MassTransit ──
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedConsumer>();
    config.AddConsumer<OrderStatusChangedConsumer>();
    config.AddConsumer<PaymentSucceededConsumer>();
    config.AddConsumer<PaymentFailedConsumer>();
    config.AddConsumer<OrderCompletedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();