using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

[Route("api/categories")]
public class CategoriesController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _catalogService.GetAllCategoriesAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}/products")]
    public async Task<IActionResult> GetProducts(Guid id)
    {
        var result = await _catalogService.GetProductsByCategoryAsync(id);
        return Ok(result);
    }
}