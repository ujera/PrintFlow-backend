using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Common;

namespace PrintFlow.Application.Interfaces.Services;

public interface ICatalogService
{
    Task<ApiResult<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<ApiResult<CategoryDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResult<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request);
    Task<ApiResult<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<ApiResult<List<ProductListDto>>> GetProductsByCategoryAsync(Guid categoryId);
    Task<ApiResult<ProductDto>> GetProductByIdAsync(Guid id);
    Task<ApiResult<ProductDto>> CreateProductAsync(CreateProductRequest request);
    Task<ApiResult<ProductDto>> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<ApiResult<bool>> DeactivateProductAsync(Guid id);
}