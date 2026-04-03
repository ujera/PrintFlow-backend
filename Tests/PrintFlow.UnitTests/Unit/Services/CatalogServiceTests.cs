using AutoMapper;
using Moq;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Mappings;
using PrintFlow.Application.Services;
using PrintFlow.Domain.Entities;

namespace PrintFlow.UnitTests.Unit.Services;

public class CatalogServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CatalogService _service;

    public CatalogServiceTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CatalogProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new CatalogService(_unitOfWork.Object, _mapper);
    }

    // ── GetAllCategoriesAsync ──

    [Fact]
    public async Task GetAllCategories_ReturnsCategories()
    {
        var categories = new List<ProductCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Business Cards", Slug = "business-cards", IsActive = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Banners", Slug = "banners", IsActive = true, DisplayOrder = 2 }
        };

        _unitOfWork.Setup(u => u.ProductCategories.GetAllActiveOrderedAsync(default))
            .ReturnsAsync(categories);

        var result = await _service.GetAllCategoriesAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    // ── CreateCategoryAsync ──

    [Fact]
    public async Task CreateCategory_ValidRequest_ReturnsCategory()
    {
        var request = new CreateCategoryRequest
        {
            Name = "New Category",
            Description = "Test",
            DisplayOrder = 1
        };

        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(), default))
            .ReturnsAsync(false);

        _unitOfWork.Setup(u => u.ProductCategories.AddAsync(It.IsAny<ProductCategory>(), default))
            .ReturnsAsync((ProductCategory c, CancellationToken _) => c);

        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.CreateCategoryAsync(request);

        Assert.True(result.Success);
        Assert.Equal("New Category", result.Data!.Name);
        Assert.Equal("new-category", result.Data.Slug);
    }

    [Fact]
    public async Task CreateCategory_DuplicateSlug_ThrowsConflict()
    {
        var request = new CreateCategoryRequest { Name = "Existing Category" };

        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(), default))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateCategoryAsync(request));
    }

    // ── GetCategoryByIdAsync ──

    [Fact]
    public async Task GetCategoryById_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();

        _unitOfWork.Setup(u => u.ProductCategories.GetByIdAsync(id, default))
            .ReturnsAsync((ProductCategory?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCategoryByIdAsync(id));
    }

    [Fact]
    public async Task GetCategoryById_Found_ReturnsCategory()
    {
        var id = Guid.NewGuid();
        var category = new ProductCategory { Id = id, Name = "Test", Slug = "test", IsActive = true };

        _unitOfWork.Setup(u => u.ProductCategories.GetByIdAsync(id, default))
            .ReturnsAsync(category);

        var result = await _service.GetCategoryByIdAsync(id);

        Assert.True(result.Success);
        Assert.Equal(id, result.Data!.Id);
    }

    // ── GetProductsByCategoryAsync ──

    [Fact]
    public async Task GetProductsByCategory_CategoryNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();

        _unitOfWork.Setup(u => u.ProductCategories.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCategory, bool>>>(), default))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProductsByCategoryAsync(id));
    }

    // ── DeactivateProductAsync ──

    [Fact]
    public async Task DeactivateProduct_SetsInactive()
    {
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Test", IsActive = true };

        _unitOfWork.Setup(u => u.Products.GetByIdAsync(id, default))
            .ReturnsAsync(product);
        _unitOfWork.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _service.DeactivateProductAsync(id);

        Assert.True(result.Success);
        Assert.False(product.IsActive);
    }

    [Fact]
    public async Task DeactivateProduct_NotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();

        _unitOfWork.Setup(u => u.Products.GetByIdAsync(id, default))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeactivateProductAsync(id));
    }
}