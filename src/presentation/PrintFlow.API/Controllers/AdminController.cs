using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Orders;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : BaseApiController
{
    private readonly ICatalogService _catalogService;
    private readonly IOrderService _orderService;

    public AdminController(ICatalogService catalogService, IOrderService orderService)
    {
        _catalogService = catalogService;
        _orderService = orderService;
    }

    // ── Dashboard ──

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _orderService.GetDashboardAsync();
        return Ok(result);
    }

    // ── Categories ──

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _catalogService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("categories/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var result = await _catalogService.GetCategoryByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _catalogService.UpdateCategoryAsync(id, request);
        return Ok(result);
    }

    // ── Products ──

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await _catalogService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, result);
    }

    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _catalogService.GetProductByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _catalogService.UpdateProductAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeactivateProduct(Guid id)
    {
        var result = await _catalogService.DeactivateProductAsync(id);
        return Ok(result);
    }

    // ── Orders ──

    [HttpGet("orders")]
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

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var result = await _orderService.GetOrderDetailAdminAsync(id);
        return Ok(result);
    }

    [HttpPut("orders/{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, GetUserId(), request);
        return Ok(result);
    }

    [HttpPost("orders/{id:guid}/approve-payment")]
    public async Task<IActionResult> ApprovePayment(Guid id)
    {
        var result = await _orderService.ApproveOfflinePaymentAsync(id, GetUserId());
        return Ok(result);
    }

    [HttpPost("orders/{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id, GetUserId());
        return Ok(result);
    }

    [HttpPost("orders/{id:guid}/refund")]
    public async Task<IActionResult> RefundOrder(Guid id)
    {
        var result = await _orderService.RefundOrderAsync(id, GetUserId());
        return Ok(result);
    }
}
