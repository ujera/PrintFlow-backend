using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// Shopping cart management — requires Customer role
/// </summary>
[Authorize(Roles = "Customer")]
[Route("api/cart")]
[Produces("application/json")]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get current user's cart with items and total
    /// </summary>
    /// <returns>Cart with items, subtotals, and total amount</returns>
    /// <response code="200">Cart details</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not a customer</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<CartDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCartAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Add a configured product to cart
    /// </summary>
    /// <param name="request">Product ID, quantity, configuration options, and optional design file URL</param>
    /// <returns>Created cart item with calculated subtotal</returns>
    /// <response code="201">Item added to cart</response>
    /// <response code="400">Invalid product or product inactive</response>
    /// <response code="404">Product not found</response>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResult<CartItemDto>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        var result = await _cartService.AddItemAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetCart), result);
    }

    /// <summary>
    /// Update cart item quantity or configuration
    /// </summary>
    /// <param name="id">Cart item ID</param>
    /// <param name="request">New quantity and/or configuration</param>
    /// <returns>Updated cart item</returns>
    /// <response code="200">Item updated</response>
    /// <response code="403">Item belongs to another user</response>
    /// <response code="404">Cart item not found</response>
    [HttpPut("items/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<CartItemDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateCartItemRequest request)
    {
        var result = await _cartService.UpdateItemAsync(GetUserId(), id, request);
        return Ok(result);
    }

    /// <summary>
    /// Remove an item from cart
    /// </summary>
    /// <param name="id">Cart item ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Item removed</response>
    /// <response code="403">Item belongs to another user</response>
    /// <response code="404">Cart item not found</response>
    [HttpDelete("items/{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveItem(Guid id)
    {
        var result = await _cartService.RemoveItemAsync(GetUserId(), id);
        return Ok(result);
    }
}