using MassTransit;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Messages;

namespace PrintFlow.Worker.Consumers;

public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(IEmailService emailService, ILogger<OrderStatusChangedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Processing status change for {OrderId}: {Old} → {New}",
            msg.OrderId, msg.OldStatus, msg.NewStatus);
        await _emailService.SendStatusChangeNotificationAsync(msg.OrderId, msg.OldStatus, msg.NewStatus);
    }
}