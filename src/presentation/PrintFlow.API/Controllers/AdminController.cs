using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Admin;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// Admin panel — dashboard, product management, order management
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : BaseApiController
{
    private readonly ICatalogService _catalogService;
    private readonly IOrderService _orderService;

    public AdminController(ICatalogService catalogService, IOrderService orderService)
    {
        _catalogService = catalogService;
        _orderService = orderService;
    }

    // ══════════════════════════════════════════
    //  DASHBOARD
    // ══════════════════════════════════════════

    /// <summary>
    /// Get admin dashboard summary
    /// </summary>
    /// <remarks>
    /// Returns 4 summary cards (awaiting payment, in production, ready for pickup,
    /// completed today) and the 10 most recent orders.
    /// </remarks>
    /// <returns>Dashboard statistics and recent orders</returns>
    /// <response code="200">Dashboard data</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResult<DashboardDto>), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _orderService.GetDashboardAsync();
        return Ok(result);
    }

    // ══════════════════════════════════════════
    //  CATEGORIES
    // ══════════════════════════════════════════

    /// <summary>
    /// Create a new product category
    /// </summary>
    /// <param name="request">Category name, description, image, and display order</param>
    /// <returns>Created category with generated slug</returns>
    /// <response code="201">Category created</response>
    /// <response code="400">Validation errors</response>
    /// <response code="409">Category slug already exists</response>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _catalogService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    [HttpGet("categories/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var result = await _catalogService.GetCategoryByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Update a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="request">Updated category data</param>
    /// <returns>Updated category</returns>
    /// <response code="200">Category updated</response>
    /// <response code="404">Category not found</response>
    /// <response code="409">New slug conflicts with existing category</response>
    [HttpPut("categories/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _catalogService.UpdateCategoryAsync(id, request);
        return Ok(result);
    }

    // ══════════════════════════════════════════
    //  PRODUCTS
    // ══════════════════════════════════════════

    /// <summary>
    /// Create a new product with options and pricing tiers
    /// </summary>
    /// <remarks>
    /// Options define configurable choices (material, size, finishing) with price modifiers.
    /// Pricing tiers define quantity-based unit pricing (e.g., 100-249 = $0.15, 250-499 = $0.12).
    /// 
    /// Option types: Material, Size, Finishing
    /// </remarks>
    /// <param name="request">Product details with options and pricing tiers</param>
    /// <returns>Created product with all details</returns>
    /// <response code="201">Product created</response>
    /// <response code="400">Validation errors</response>
    /// <response code="404">Category not found</response>
    /// <response code="409">Product slug already exists</response>
    [HttpPost("products")]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await _catalogService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product with options and pricing tiers</returns>
    /// <response code="200">Product found</response>
    /// <response code="404">Product not found</response>
    [HttpGet("products/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _catalogService.GetProductByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Update a product — replaces all options and pricing tiers
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Updated product data with full options and pricing</param>
    /// <returns>Updated product</returns>
    /// <response code="200">Product updated</response>
    /// <response code="404">Product or category not found</response>
    /// <response code="409">New slug conflicts with existing product</response>
    [HttpPut("products/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _catalogService.UpdateProductAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Soft-delete a product (sets IsActive = false)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Product deactivated</response>
    /// <response code="404">Product not found</response>
    [HttpDelete("products/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeactivateProduct(Guid id)
    {
        var result = await _catalogService.DeactivateProductAsync(id);
        return Ok(result);
    }

    // ══════════════════════════════════════════
    //  ORDERS
    // ══════════════════════════════════════════

    /// <summary>
    /// List all orders with filtering and pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <param name="status">Filter by status (e.g., Paid, InProduction)</param>
    /// <param name="fromDate">Filter orders from this date</param>
    /// <param name="toDate">Filter orders until this date</param>
    /// <param name="searchTerm">Search by customer name or email</param>
    /// <returns>Paginated order list</returns>
    /// <response code="200">Paginated orders</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(ApiResult<PagedResponse<OrderListDto>>), 200)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _orderService.GetAllOrdersAsync(
            pageNumber, pageSize, status, fromDate, toDate, searchTerm);
        return Ok(result);
    }

    /// <summary>
    /// Get full order details including customer, payments, and status history
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Complete order details</returns>
    /// <response code="200">Order details</response>
    /// <response code="404">Order not found</response>
    [HttpGet("orders/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<OrderDetailDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var result = await _orderService.GetOrderDetailAdminAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Advance order to the next status
    /// </summary>
    /// <remarks>
    /// Valid transitions:
    /// Paid → InProduction → QualityCheck → ReadyForPickup → Completed
    /// 
    /// Invalid transitions return 400 with the current and attempted status.
    /// Each status change is logged in OrderStatusHistory and triggers an email notification.
    /// </remarks>
    /// <param name="id">Order ID</param>
    /// <param name="request">New status and optional notes</param>
    /// <returns>Updated order</returns>
    /// <response code="200">Status updated</response>
    /// <response code="400">Invalid status transition</response>
    /// <response code="404">Order not found</response>
    [HttpPut("orders/{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, GetUserId(), request);
        return Ok(result);
    }

    /// <summary>
    /// Approve an offline bank transfer payment
    /// </summary>
    /// <remarks>
    /// Only works for orders with status AwaitingPayment.
    /// Sets order to Paid, creates a payment record, and sends confirmation email.
    /// </remarks>
    /// <param name="id">Order ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Payment approved</response>
    /// <response code="400">Order not awaiting payment</response>
    /// <response code="404">Order not found</response>
    [HttpPost("orders/{id:guid}/approve-payment")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ApprovePayment(Guid id)
    {
        var result = await _orderService.ApproveOfflinePaymentAsync(id, GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <remarks>
    /// Cannot cancel completed orders. Sends cancellation notification email.
    /// For refunds on card-paid orders, use the /refund endpoint instead.
    /// </remarks>
    /// <param name="id">Order ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Order cancelled</response>
    /// <response code="400">Cannot cancel (order completed or already cancelled)</response>
    /// <response code="404">Order not found</response>
    [HttpPost("orders/{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id, GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Refund a card-paid order via Stripe
    /// </summary>
    /// <remarks>
    /// Processes refund through Stripe API, creates a refund payment record,
    /// and cancels the order. Only works for orders with a successful card payment.
    /// </remarks>
    /// <param name="id">Order ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Order refunded</response>
    /// <response code="400">No successful payment to refund</response>
    /// <response code="402">Stripe refund failed</response>
    /// <response code="404">Order not found</response>
    [HttpPost("orders/{id:guid}/refund")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(402)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RefundOrder(Guid id)
    {
        var result = await _orderService.RefundOrderAsync(id, GetUserId());
        return Ok(result);
    }
}