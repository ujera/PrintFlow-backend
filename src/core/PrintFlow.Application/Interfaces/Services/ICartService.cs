using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Application.DTOs.Common;

namespace PrintFlow.Application.Interfaces.Services;

public interface ICartService
{
    Task<ApiResult<CartDto>> GetCartAsync(Guid userId);
    Task<ApiResult<CartItemDto>> AddItemAsync(Guid userId, AddCartItemRequest request);
    Task<ApiResult<CartItemDto>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request);
    Task<ApiResult<bool>> RemoveItemAsync(Guid userId, Guid itemId);
}