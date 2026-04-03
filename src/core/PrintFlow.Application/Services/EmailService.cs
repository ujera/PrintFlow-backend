using Microsoft.Extensions.Logging;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PrintFlow.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SendOrderConfirmationAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order?.User?.Email is null) return;

            var subject = $"PrintFlow — Order #{order.Id.ToString()[..8]} Confirmed";
            var body = $"""
            Hi {order.User.Name},
 
            Your order has been placed successfully.
 
            Order ID: {order.Id}
            Total: ${order.TotalAmount:F2}
            Items: {order.Items.Count}
            Payment Method: {order.PaymentMethod}
 
            {(order.PaymentMethod == Domain.Enums.PaymentMethod.BankTransfer
                    ? "Please complete your bank transfer to proceed."
                    : "Your card payment is being processed.")}
 
            Thank you for choosing PrintFlow!
            """;

            await SendEmailAsync(order.User.Email, subject, body);
            await LogNotificationAsync(order.UserId, orderId, subject, body);
        }

        public async Task SendStatusChangeNotificationAsync(Guid orderId, string oldStatus, string newStatus)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order?.User?.Email is null) return;

            var subject = $"PrintFlow — Order #{order.Id.ToString()[..8]} Status Updated";
            var body = $"""
            Hi {order.User.Name},
 
            Your order status has been updated.
 
            Order ID: {order.Id}
            Previous Status: {oldStatus}
            New Status: {newStatus}
 
            {GetStatusMessage(newStatus)}
 
            Thank you for choosing PrintFlow!
            """;

            await SendEmailAsync(order.User.Email, subject, body);
            await LogNotificationAsync(order.UserId, orderId, subject, body);
        }

        public async Task SendPaymentConfirmationAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order?.User?.Email is null) return;

            var subject = $"PrintFlow — Payment Confirmed for Order #{order.Id.ToString()[..8]}";
            var body = $"""
            Hi {order.User.Name},
 
            Your payment of ${order.TotalAmount:F2} has been confirmed.
 
            Order ID: {order.Id}
            Your order is now being prepared for production.
 
            Thank you for choosing PrintFlow!
            """;

            await SendEmailAsync(order.User.Email, subject, body);
            await LogNotificationAsync(order.UserId, orderId, subject, body);
        }

        public async Task SendPaymentFailedReminderAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order?.User?.Email is null) return;

            var subject = $"PrintFlow — Payment Failed for Order #{order.Id.ToString()[..8]}";
            var body = $"""
            Hi {order.User.Name},
 
            Unfortunately, your payment for order #{order.Id.ToString()[..8]} could not be processed.
 
            Total: ${order.TotalAmount:F2}
 
            Please retry your payment to proceed with your order.
 
            Thank you for choosing PrintFlow!
            """;

            await SendEmailAsync(order.User.Email, subject, body);
            await LogNotificationAsync(order.UserId, orderId, subject, body);
        }

        public async Task SendInvoiceEmailAsync(Guid orderId, byte[] pdfBytes)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order?.User?.Email is null) return;

            var subject = $"PrintFlow — Invoice for Order #{order.Id.ToString()[..8]}";
            var body = $"""
            Hi {order.User.Name},
 
            Your order #{order.Id.ToString()[..8]} is now complete.
            Please find your invoice attached.
 
            Total: ${order.TotalAmount:F2}
 
            Thank you for choosing PrintFlow!
            """;

            await SendEmailAsync(order.User.Email, subject, body, pdfBytes, $"Invoice_{order.Id.ToString()[..8]}.pdf");
            await LogNotificationAsync(order.UserId, orderId, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body,
            byte[]? attachment = null, string? attachmentName = null)
        {
            var smtpSettings = _configuration.GetSection("Smtp");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var from = smtpSettings["From"] ?? "noreply@printflow.io";

            if (string.IsNullOrEmpty(host))
            {
                _logger.LogWarning("SMTP not configured. Email to {To} with subject '{Subject}' was not sent.", to, subject);
                return;
            }

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var message = new MailMessage(from, to, subject, body);

                if (attachment is not null && attachmentName is not null)
                {
                    var stream = new MemoryStream(attachment);
                    message.Attachments.Add(new Attachment(stream, attachmentName, "application/pdf"));
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            }
        }

        private async Task LogNotificationAsync(Guid userId, Guid orderId, string subject, string body)
        {
            await _unitOfWork.Notifications.AddAsync(new Domain.Entities.Notification
            {
                UserId = userId,
                OrderId = orderId,
                Type = Domain.Enums.NotificationType.Email,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow,
                Status = Domain.Enums.NotificationStatus.Sent
            });
            await _unitOfWork.SaveChangesAsync();
        }

        private static string GetStatusMessage(string status) => status switch
        {
            "Paid" => "Your payment has been confirmed. Production will begin shortly.",
            "InProduction" => "Your order is now being produced.",
            "QualityCheck" => "Your order is undergoing quality checks.",
            "ReadyForPickup" => "Your order is ready for pickup!",
            "Completed" => "Your order has been completed. Thank you!",
            "Cancelled" => "Your order has been cancelled.",
            _ => "Please check your order for details."
        };
    }

}
