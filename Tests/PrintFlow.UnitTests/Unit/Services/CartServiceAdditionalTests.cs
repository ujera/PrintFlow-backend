using AutoMapper;
using Moq;
using PrintFlow.Application.DTOs.Cart;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Mappings;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Entities;

namespace PrintFlow.UnitTests.Unit.Services;

public class CartServiceAdditionalTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CartService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public CartServiceAdditionalTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CartProfile>());
        _mapper = config.CreateMapper();
        _service = new CartService(_unitOfWork.Object, _mapper);
    }

    [Fact]
    public async Task UpdateItem_OwnItem_UpdatesQuantity()
    {
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Card", BasePrice = 10, PricingTiers = new List<PricingTier>() };
        var cartItem = new CartItem { Id = itemId, UserId = _userId, ProductId = productId, Quantity = 5 };

        _unitOfWork.Setup(u => u.CartItems.GetByIdAsync(itemId, default)).ReturnsAsync(cartItem);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default))
            .ReturnsAsync(new List<CartItem>
            {
                new() { Id = itemId, UserId = _userId, ProductId = productId, Product = product, Quantity = 10 }
            });

        var result = await _service.UpdateItemAsync(_userId, itemId, new UpdateCartItemRequest { Quantity = 10 });

        Assert.True(result.Success);
        Assert.Equal(10, cartItem.Quantity);
    }

    [Fact]
    public async Task GetCart_MultipleItems_CalculatesCorrectTotal()
    {
        var product1 = new Product { Id = Guid.NewGuid(), Name = "Card", BasePrice = 10, PricingTiers = new List<PricingTier>() };
        var product2 = new Product { Id = Guid.NewGuid(), Name = "Banner", BasePrice = 25, PricingTiers = new List<PricingTier>() };

        var items = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product1.Id, Product = product1, Quantity = 3 },
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product2.Id, Product = product2, Quantity = 2 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default)).ReturnsAsync(items);

        var result = await _service.GetCartAsync(_userId);

        // 3 × 10 + 2 × 25 = 80
        Assert.Equal(80m, result.Data!.Total);
        Assert.Equal(2, result.Data.Items.Count);
    }

    [Fact]
    public async Task GetCart_ProductWithNullPricingTiers_UsesBasePrice()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Sticker", BasePrice = 0.50m, PricingTiers = null! };
        var items = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), UserId = _userId, ProductId = product.Id, Product = product, Quantity = 100 }
        };

        _unitOfWork.Setup(u => u.CartItems.GetByUserIdWithProductAsync(_userId, default)).ReturnsAsync(items);

        var result = await _service.GetCartAsync(_userId);

        Assert.Equal(50m, result.Data!.Total);
    }
}