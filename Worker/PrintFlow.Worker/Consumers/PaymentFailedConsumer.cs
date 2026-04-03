using MassTransit;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Messages;

namespace PrintFlow.Worker.Consumers;

public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(IEmailService emailService, ILogger<PaymentFailedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        _logger.LogInformation("Processing payment failure for {OrderId}", context.Message.OrderId);
        await _emailService.SendPaymentFailedReminderAsync(context.Message.OrderId);
    }
}