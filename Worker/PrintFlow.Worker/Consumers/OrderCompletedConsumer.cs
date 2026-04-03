using MassTransit;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Application.Messages;

namespace PrintFlow.Worker.Consumers;

public class OrderCompletedConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly IInvoiceService _invoiceService;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCompletedConsumer> _logger;

    public OrderCompletedConsumer(
        IInvoiceService invoiceService,
        IEmailService emailService,
        ILogger<OrderCompletedConsumer> logger)
    {
        _invoiceService = invoiceService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var orderId = context.Message.OrderId;
        _logger.LogInformation("Generating invoice for completed order {OrderId}", orderId);

        var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(orderId);
        await _emailService.SendInvoiceEmailAsync(orderId, pdfBytes);

        _logger.LogInformation("Invoice sent for order {OrderId}", orderId);
    }
}