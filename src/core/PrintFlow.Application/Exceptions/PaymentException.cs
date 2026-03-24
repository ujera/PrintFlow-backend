namespace PrintFlow.Application.Exceptions;

public class PaymentException : AppException
{
    public string? StripeErrorCode { get; }

    public PaymentException(string message, string? stripeErrorCode = null)
        : base(message, 402)
    {
        StripeErrorCode = stripeErrorCode;
    }
}