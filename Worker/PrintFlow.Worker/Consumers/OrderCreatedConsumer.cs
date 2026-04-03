using MassTransit;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Messages;

namespace PrintFlow.Worker.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IEmailService emailService, ILogger<OrderCreatedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation("Processing order confirmation for {OrderId}", context.Message.OrderId);
        await _emailService.SendOrderConfirmationAsync(context.Message.OrderId);
    }
}