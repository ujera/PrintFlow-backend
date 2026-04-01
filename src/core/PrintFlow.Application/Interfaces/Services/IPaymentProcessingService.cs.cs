namespace PrintFlow.Application.Interfaces.Services;

public interface IPaymentProcessingService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Guid orderId, decimal amount, string currency = "usd");
    Task<RefundResult> RefundPaymentAsync(string stripePaymentId, decimal amount);
}

public class PaymentIntentResult
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class RefundResult
{
    public bool Success { get; set; }
    public string RefundId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}