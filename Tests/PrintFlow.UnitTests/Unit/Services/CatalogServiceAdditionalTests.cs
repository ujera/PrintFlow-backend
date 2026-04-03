using System.Linq.Expressions;
using AutoMapper;
using Moq;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Mappings;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Entities;

namespace PrintFlow.UnitTests.Unit.Services;

public class CatalogServiceAdditionalTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CatalogService _service;

    public CatalogServiceAdditionalTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CatalogProfile>());
        _mapper = config.CreateMapper();
        _service = new CatalogService(_unitOfWork.Object, _mapper);
    }

    // ── UpdateCategoryAsync ──

    [Fact]
    public async Task UpdateCategory_NotFound_ThrowsNotFoundException()
    {
        _unitOfWork.Setup(u => u.ProductCategories.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ProductCategory?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateCategoryAsync(Guid.NewGuid(), new UpdateCategoryRequest { Name = "Test" }));
    }

    [Fact]
    public async Task UpdateCategory_ValidRequest_UpdatesFields()
    {
        var id = Guid.NewGuid();
        var existing = new ProductCategory { Id = id, Name = "Old", Slug = "old", IsActive = true, DisplayOrder = 1 };

        _unitOfWork.Setup(u => u.ProductCategories.GetByIdAsync(id, default)).ReturnsAsync(existing);
        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>(), default)).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var request = new UpdateCategoryRequest { Name = "New Name", Description = "New desc", DisplayOrder = 5, IsActive = false };
        var result = await _service.UpdateCategoryAsync(id, request);

        Assert.True(result.Success);
        Assert.Equal("New Name", existing.Name);
        Assert.Equal("new-name", existing.Slug);
        Assert.Equal("New desc", existing.Description);
        Assert.Equal(5, existing.DisplayOrder);
        Assert.False(existing.IsActive);
    }

    [Fact]
    public async Task UpdateCategory_SlugConflict_ThrowsConflict()
    {
        var id = Guid.NewGuid();
        var existing = new ProductCategory { Id = id, Name = "Old", Slug = "old" };

        _unitOfWork.Setup(u => u.ProductCategories.GetByIdAsync(id, default)).ReturnsAsync(existing);
        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>(), default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.UpdateCategoryAsync(id, new UpdateCategoryRequest { Name = "Existing Name" }));
    }

    // ── GetProductByIdAsync ──

    [Fact]
    public async Task GetProductById_Found_ReturnsProduct()
    {
        var id = Guid.NewGuid();
        var product = new Product
        {
            Id = id,
            Name = "Test Product",
            Slug = "test-product",
            BasePrice = 10,
            Category = new ProductCategory { Name = "Test Category" },
            Options = new List<ProductOption>(),
            PricingTiers = new List<PricingTier>()
        };

        _unitOfWork.Setup(u => u.Products.GetWithDetailsAsync(id, default)).ReturnsAsync(product);

        var result = await _service.GetProductByIdAsync(id);

        Assert.True(result.Success);
        Assert.Equal("Test Product", result.Data!.Name);
        Assert.Equal("Test Category", result.Data.CategoryName);
    }

    [Fact]
    public async Task GetProductById_NotFound_ThrowsNotFoundException()
    {
        _unitOfWork.Setup(u => u.Products.GetWithDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetProductByIdAsync(Guid.NewGuid()));
    }

    // ── GetProductsByCategoryAsync ──

    [Fact]
    public async Task GetProductsByCategory_ReturnsProducts()
    {
        var categoryId = Guid.NewGuid();

        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>(), default)).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Products.GetActiveByCategoryIdAsync(categoryId, default))
            .ReturnsAsync(new List<Product>
            {
                new() { Id = Guid.NewGuid(), Name = "Product 1", Slug = "product-1", BasePrice = 10 },
                new() { Id = Guid.NewGuid(), Name = "Product 2", Slug = "product-2", BasePrice = 20 }
            });

        var result = await _service.GetProductsByCategoryAsync(categoryId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    // ── CreateProductAsync ──

    [Fact]
    public async Task CreateProduct_CategoryNotFound_ThrowsNotFoundException()
    {
        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>(), default)).ReturnsAsync(false);

        var request = new CreateProductRequest
        {
            CategoryId = Guid.NewGuid(),
            Name = "Test",
            BasePrice = 10
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateProductAsync(request));
    }

    [Fact]
    public async Task CreateProduct_DuplicateSlug_ThrowsConflict()
    {
        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>(), default)).ReturnsAsync(true);
        _unitOfWork.Setup(u => u.Products.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateProductAsync(new CreateProductRequest { CategoryId = Guid.NewGuid(), Name = "Existing", BasePrice = 10 }));
    }
}