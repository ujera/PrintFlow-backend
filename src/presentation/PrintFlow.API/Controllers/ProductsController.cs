using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// Public product details with options and pricing tiers
/// </summary>
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public ProductsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Get product details with configurable options and quantity-based pricing
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Full product details including options, pricing tiers, and category</returns>
    /// <response code="200">Product details</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _catalogService.GetProductByIdAsync(id);
        return Ok(result);
    }
}