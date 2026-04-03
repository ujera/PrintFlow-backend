using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// Public product catalog — categories and products browsing
/// </summary>
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// List all active product categories
    /// </summary>
    /// <returns>Categories ordered by display order</returns>
    /// <response code="200">List of categories</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<CategoryDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _catalogService.GetAllCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// List all active products in a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Products with base pricing</returns>
    /// <response code="200">List of products</response>
    /// <response code="404">Category not found</response>
    [HttpGet("{id:guid}/products")]
    [ProducesResponseType(typeof(ApiResult<List<ProductListDto>>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProducts(Guid id)
    {
        var result = await _catalogService.GetProductsByCategoryAsync(id);
        return Ok(result);
    }
}