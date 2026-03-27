using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

[Route("api/products")]
public class ProductsController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public ProductsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _catalogService.GetProductByIdAsync(id);
        return Ok(result);
    }
}