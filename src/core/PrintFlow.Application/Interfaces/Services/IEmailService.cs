namespace PrintFlow.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(Guid orderId);
    Task SendStatusChangeNotificationAsync(Guid orderId, string oldStatus, string newStatus);
    Task SendPaymentConfirmationAsync(Guid orderId);
    Task SendPaymentFailedReminderAsync(Guid orderId);
    Task SendInvoiceEmailAsync(Guid orderId, byte[] pdfBytes);
}