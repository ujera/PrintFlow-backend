using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Interfaces.Services;
 
namespace PrintFlow.API.Controllers;

/// <summary>
/// Customer order management — create orders, track status, pay, download invoices
/// </summary>
[Authorize(Roles = "Customer")]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IInvoiceService _invoiceService;

    public OrdersController(IOrderService orderService, IInvoiceService invoiceService)
    {
        _orderService = orderService;
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Create a new order from the current cart
    /// </summary>
    /// <remarks>
    /// Converts all cart items into order items, calculates totals using pricing tiers,
    /// clears the cart, and publishes an OrderCreatedEvent for email notification.
    /// 
    /// Payment methods:
    /// - **Card** — order status becomes Created, call /pay to initiate Stripe payment
    /// - **BankTransfer** — order status becomes AwaitingPayment, admin approves manually
    /// </remarks>
    /// <param name="request">Payment method (Card or BankTransfer) and optional notes</param>
    /// <returns>Created order with items and totals</returns>
    /// <response code="201">Order created</response>
    /// <response code="400">Cart is empty or invalid payment method</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// List all orders for the current customer
    /// </summary>
    /// <returns>Orders sorted by newest first</returns>
    /// <response code="200">List of orders</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<OrderListDto>>), 200)]
    public async Task<IActionResult> GetMyOrders()
    {
        var result = await _orderService.GetMyOrdersAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Get detailed order information
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order with items, payments, status history, and customer info</returns>
    /// <response code="200">Order details</response>
    /// <response code="403">Order belongs to another user</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<OrderDetailDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(GetUserId(), id);
        return Ok(result);
    }

    /// <summary>
    /// Initiate Stripe card payment for an order
    /// </summary>
    /// <remarks>
    /// Creates a Stripe PaymentIntent and returns the clientSecret.
    /// The frontend uses this with Stripe.js to confirm payment.
    /// Only works for orders with Card payment method and status Created or PaymentFailed.
    /// </remarks>
    /// <param name="id">Order ID</param>
    /// <returns>Stripe clientSecret for frontend payment confirmation</returns>
    /// <response code="200">Payment intent created</response>
    /// <response code="400">Order not eligible for payment</response>
    /// <response code="402">Stripe payment error</response>
    /// <response code="403">Order belongs to another user</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(ApiResult<PaymentIntentDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(402)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> InitiatePayment(Guid id)
    {
        var result = await _orderService.InitiatePaymentAsync(GetUserId(), id);
        return Ok(result);
    }

    /// <summary>
    /// Download invoice PDF for an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>PDF file download</returns>
    /// <response code="200">Invoice PDF</response>
    /// <response code="403">Order belongs to another user</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id:guid}/invoice")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DownloadInvoice(Guid id)
    {
        await _orderService.GetOrderByIdAsync(GetUserId(), id);
        var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(id);
        return File(pdfBytes, "application/pdf", $"Invoice_{id.ToString()[..8]}.pdf");
    }
}