using AutoMapper;
using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResult<CartDto>> GetCartAsync(Guid userId)
    {
        var items = await _unitOfWork.CartItems.GetByUserIdWithProductAsync(userId);
        var itemDtos = _mapper.Map<List<CartItemDto>>(items);

        foreach (var dto in itemDtos)
        {
            var item = items.First(i => i.Id == dto.Id);
            dto.Subtotal = CalculateSubtotal(item);
        }

        return ApiResult<CartDto>.Ok(new CartDto
        {
            Items = itemDtos,
            Total = itemDtos.Sum(i => i.Subtotal)
        });
    }

    public async Task<ApiResult<CartItemDto>> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (!product.IsActive)
            throw new BadRequestException("Product is not available.");

        var cartItem = _mapper.Map<CartItem>(request);
        cartItem.UserId = userId;

        await _unitOfWork.CartItems.AddAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();

        var saved = (await _unitOfWork.CartItems.GetByUserIdWithProductAsync(userId))
            .First(c => c.Id == cartItem.Id);

        var dto = _mapper.Map<CartItemDto>(saved);
        dto.Subtotal = CalculateSubtotal(saved);

        return ApiResult<CartItemDto>.Ok(dto, "Item added to cart.");
    }

    public async Task<ApiResult<CartItemDto>> UpdateItemAsync(Guid userId, Guid itemId, UpdateCartItemRequest request)
    {
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(itemId)
            ?? throw new NotFoundException("CartItem", itemId);

        if (cartItem.UserId != userId)
            throw new ForbiddenException();

        cartItem.Quantity = request.Quantity;
        cartItem.ConfigJson = request.ConfigJson ?? cartItem.ConfigJson;

        _unitOfWork.CartItems.Update(cartItem);
        await _unitOfWork.SaveChangesAsync();

        var saved = (await _unitOfWork.CartItems.GetByUserIdWithProductAsync(userId))
            .First(c => c.Id == cartItem.Id);

        var dto = _mapper.Map<CartItemDto>(saved);
        dto.Subtotal = CalculateSubtotal(saved);

        return ApiResult<CartItemDto>.Ok(dto, "Cart item updated.");
    }

    public async Task<ApiResult<bool>> RemoveItemAsync(Guid userId, Guid itemId)
    {
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(itemId)
            ?? throw new NotFoundException("CartItem", itemId);

        if (cartItem.UserId != userId)
            throw new ForbiddenException();

        _unitOfWork.CartItems.Remove(cartItem);
        await _unitOfWork.SaveChangesAsync();

        return ApiResult<bool>.Ok(true, "Item removed from cart.");
    }

    private static decimal CalculateSubtotal(CartItem item)
    {
        if (item.Product is null) return 0;

        var tier = item.Product.PricingTiers?
            .Where(t => item.Quantity >= t.MinQuantity && item.Quantity <= t.MaxQuantity)
            .FirstOrDefault();

        var unitPrice = tier?.UnitPrice ?? item.Product.BasePrice;
        return unitPrice * item.Quantity;
    }
}