using Microsoft.Extensions.Configuration;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Services;
using Stripe;


namespace PrintFlow.Infrastructure.Services
{
    public class StripePaymentService : IPaymentProcessingService
    {
        public StripePaymentService(IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Guid orderId, decimal amount, string currency = "usd")
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = currency,
                    Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() }
                },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return new PaymentIntentResult
                {
                    PaymentIntentId = intent.Id,
                    ClientSecret = intent.ClientSecret,
                    Status = intent.Status
                };
            }
            catch (StripeException ex)
            {
                throw new PaymentException(ex.Message, ex.StripeError?.Code);
            }
        }

        public async Task<RefundResult> RefundPaymentAsync(string stripePaymentId, decimal amount)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = stripePaymentId,
                    Amount = (long)(amount * 100)
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

                return new RefundResult
                {
                    Success = refund.Status == "succeeded",
                    RefundId = refund.Id,
                    Status = refund.Status
                };
            }
            catch (StripeException ex)
            {
                throw new PaymentException(ex.Message, ex.StripeError?.Code);
            }
        }
    }
}
