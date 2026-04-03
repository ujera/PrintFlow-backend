
using AutoMapper;
using Moq;
using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Mappings;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Entities;

namespace PrintFlow.UnitTests.Unit.Services;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CartService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public CartServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CartProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new CartService(_unitOfWork.Object, _mapper);
    }

    // ── GetCartAsync ──

    [Fact]
    public async Task GetCart_EmptyCart_ReturnsZeroTotal()
    {
        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(new List<CartItem>());

        var result = await _service.GetCartAsync(_userId);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.Items);
        Assert.Equal(0, result.Data.Total);
    }

    [Fact]
    public async Task GetCart_WithItems_CalculatesTotalFromPricingTiers()
    {
        var product = new Product
        {
            Id = _productId,
            Name = "Business Card",
            BasePrice = 0.15m,
            PricingTiers = new List<PricingTier>
            {
                new() { MinQuantity = 100, MaxQuantity = 249, UnitPrice = 0.15m },
                new() { MinQuantity = 250, MaxQuantity = 499, UnitPrice = 0.12m }
            }
        };

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = _productId, Product = product, Quantity = 250 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(cartItems);

        var result = await _service.GetCartAsync(_userId);

        Assert.True(result.Success);
        Assert.Single(result.Data!.Items);
        // 250 qty hits the 250-499 tier at $0.12 each = $30.00
        Assert.Equal(30.00m, result.Data.Total);
    }

    [Fact]
    public async Task GetCart_QuantityBelowAllTiers_UsesBasePrice()
    {
        var product = new Product
        {
            Id = _productId,
            Name = "Business Card",
            BasePrice = 0.20m,
            PricingTiers = new List<PricingTier>
            {
                new() { MinQuantity = 100, MaxQuantity = 500, UnitPrice = 0.10m }
            }
        };

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = _productId, Product = product, Quantity = 50 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(cartItems);

        var result = await _service.GetCartAsync(_userId);

        // 50 qty doesn't match any tier → base price 0.20 × 50 = 10.00
        Assert.Equal(10.00m, result.Data!.Total);
    }

    // ── AddItemAsync ──

    [Fact]
    public async Task AddItem_ProductNotFound_ThrowsNotFoundException()
    {
        var request = new AddCartItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 };

        _unitOfWork.Setup(u => u.Products.GetByIdAsync(request.ProductId, default))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddItemAsync(_userId, request));
    }

    [Fact]
    public async Task AddItem_InactiveProduct_ThrowsBadRequest()
    {
        var request = new AddCartItemRequest { ProductId = _productId, Quantity = 1 };

        _unitOfWork.Setup(u => u.Products.GetByIdAsync(_productId, default))
            .ReturnsAsync(new Product { Id = _productId, IsActive = false });

        await Assert.ThrowsAsync<BadRequestException>(() => _service.AddItemAsync(_userId, request));
    }

    // ── RemoveItemAsync ──

    [Fact]
    public async Task RemoveItem_NotFound_ThrowsNotFoundException()
    {
        var itemId = Guid.NewGuid();

        _unitOfWork.Setup(u => u.CartItems.GetByIdAsync(itemId, default))
            .ReturnsAsync((CartItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoveItemAsync(_userId, itemId));
    }

    [Fact]
    public async Task RemoveItem_BelongsToDifferentUser_ThrowsForbidden()
    {
        var itemId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _unitOfWork.Setup(u => u.CartItems.GetByIdAsync(itemId, default))
            .ReturnsAsync(new CartItem { Id = itemId, UserId = otherUserId });

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.RemoveItemAsync(_userId, itemId));
    }

    [Fact]
    public async Task RemoveItem_OwnItem_Succeeds()
    {
        var itemId = Guid.NewGuid();

        _unitOfWork.Setup(u => u.CartItems.GetByIdAsync(itemId, default))
            .ReturnsAsync(new CartItem { Id = itemId, UserId = _userId });
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.RemoveItemAsync(_userId, itemId);

        Assert.True(result.Success);
    }

    // ── UpdateItemAsync ──

    [Fact]
    public async Task UpdateItem_BelongsToDifferentUser_ThrowsForbidden()
    {
        var itemId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var request = new UpdateCartItemRequest { Quantity = 5 };

        _unitOfWork.Setup(u => u.CartItems.GetByIdAsync(itemId, default))
            .ReturnsAsync(new CartItem { Id = itemId, UserId = otherUserId });

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateItemAsync(_userId, itemId, request));
    }
}