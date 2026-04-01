using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.Interfaces.Services;
using Stripe;

namespace PrintFlow.API.Controllers;

[Route("api/webhooks/stripe")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger — not meant to be called manually
public class StripeWebhookController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IOrderService orderService,
        IConfiguration configuration,
        ILogger<StripeWebhookController> logger)
    {
        _orderService = orderService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signatureHeader))
        {
            _logger.LogWarning("Stripe webhook received without signature header.");
            return BadRequest("Missing Stripe-Signature header.");
        }

        var webhookSecret = _configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogError("Stripe webhook secret is not configured.");
            return StatusCode(500, "Webhook secret not configured.");
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning("Stripe webhook signature verification failed: {Message}", ex.Message);
            return BadRequest("Invalid signature.");
        }

        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                var successIntent = stripeEvent.Data.Object as PaymentIntent;
                if (successIntent is not null)
                {
                    _logger.LogInformation("Payment succeeded: {Id}", successIntent.Id);
                    await _orderService.HandlePaymentSucceededAsync(successIntent.Id);
                }
                break;

            case EventTypes.PaymentIntentPaymentFailed:
                var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                if (failedIntent is not null)
                {
                    _logger.LogInformation("Payment failed: {Id}", failedIntent.Id);
                    await _orderService.HandlePaymentFailedAsync(failedIntent.Id);
                }
                break;

            default:
                _logger.LogInformation("Unhandled Stripe event: {Type}", stripeEvent.Type);
                break;
        }

        return Ok();
    }
}