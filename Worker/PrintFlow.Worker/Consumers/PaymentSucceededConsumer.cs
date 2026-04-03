using MassTransit;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Messages;

namespace PrintFlow.Worker.Consumers;

public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentSucceededConsumer> _logger;

    public PaymentSucceededConsumer(IEmailService emailService, ILogger<PaymentSucceededConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        _logger.LogInformation("Processing payment confirmation for {OrderId}", context.Message.OrderId);
        await _emailService.SendPaymentConfirmationAsync(context.Message.OrderId);
    }
}