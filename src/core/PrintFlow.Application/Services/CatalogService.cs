using AutoMapper;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Exceptions;
using PrintFlow.Application.Interfaces.Repositories;
using PrintFlow.Application.Interfaces.Services;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Application.Services;

public class CatalogService : ICatalogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CatalogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // ── Categories ──

    public async Task<ApiResult<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        var categories = await _unitOfWork.ProductCategories.GetAllActiveOrderedAsync();
        return ApiResult<List<CategoryDto>>.Ok(_mapper.Map<List<CategoryDto>>(categories));
    }

    public async Task<ApiResult<CategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        var category = await _unitOfWork.ProductCategories.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        return ApiResult<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category));
    }

    public async Task<ApiResult<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var slug = SlugGenerator.Generate(request.Name);

        if (await _unitOfWork.ProductCategories.AnyAsync(c => c.Slug == slug))
            throw new ConflictException("Category", "slug", slug);

        var category = _mapper.Map<ProductCategory>(request);
        category.Slug = slug;

        await _unitOfWork.ProductCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return ApiResult<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category), "Category created.");
    }

    public async Task<ApiResult<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _unitOfWork.ProductCategories.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        var newSlug = SlugGenerator.Generate(request.Name);

        if (newSlug != category.Slug && await _unitOfWork.ProductCategories.AnyAsync(c => c.Slug == newSlug))
            throw new ConflictException("Category", "slug", newSlug);

        category.Name = request.Name;
        category.Slug = newSlug;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        _unitOfWork.ProductCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return ApiResult<CategoryDto>.Ok(_mapper.Map<CategoryDto>(category), "Category updated.");
    }

    // ── Products ──

    public async Task<ApiResult<List<ProductListDto>>> GetProductsByCategoryAsync(Guid categoryId)
    {
        if (!await _unitOfWork.ProductCategories.AnyAsync(c => c.Id == categoryId))
            throw new NotFoundException("Category", categoryId);

        var products = await _unitOfWork.Products.GetActiveByCategoryIdAsync(categoryId);
        return ApiResult<List<ProductListDto>>.Ok(_mapper.Map<List<ProductListDto>>(products));
    }

    public async Task<ApiResult<ProductDto>> GetProductByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsAsync(id)
            ?? throw new NotFoundException("Product", id);

        return ApiResult<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }

    public async Task<ApiResult<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        if (!await _unitOfWork.ProductCategories.AnyAsync(c => c.Id == request.CategoryId))
            throw new NotFoundException("Category", request.CategoryId);

        var slug = SlugGenerator.Generate(request.Name);

        if (await _unitOfWork.Products.AnyAsync(p => p.Slug == slug))
            throw new ConflictException("Product", "slug", slug);

        var product = _mapper.Map<Product>(request);
        product.Slug = slug;

        // Map options
        foreach (var optReq in request.Options)
        {
            var option = _mapper.Map<ProductOption>(optReq);
            option.OptionType = Enum.Parse<OptionType>(optReq.OptionType, true);
            product.Options.Add(option);
        }

        // Map pricing tiers
        foreach (var tierReq in request.PricingTiers)
        {
            product.PricingTiers.Add(_mapper.Map<PricingTier>(tierReq));
        }

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
        return ApiResult<ProductDto>.Ok(_mapper.Map<ProductDto>(created!), "Product created.");
    }

    public async Task<ApiResult<ProductDto>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _unitOfWork.Products.GetByIdWithIncludesAsync(id,
            default, p => p.Options, p => p.PricingTiers)
            ?? throw new NotFoundException("Product", id);

        if (!await _unitOfWork.ProductCategories.AnyAsync(c => c.Id == request.CategoryId))
            throw new NotFoundException("Category", request.CategoryId);

        var newSlug = SlugGenerator.Generate(request.Name);

        if (newSlug != product.Slug && await _unitOfWork.Products.AnyAsync(p => p.Slug == newSlug))
            throw new ConflictException("Product", "slug", newSlug);

        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.Slug = newSlug;
        product.Description = request.Description;
        product.BasePrice = request.BasePrice;
        product.IsActive = request.IsActive;

        // Replace options
        _unitOfWork.ProductOptions.RemoveRange(product.Options);
        foreach (var optReq in request.Options)
        {
            var option = _mapper.Map<ProductOption>(optReq);
            option.ProductId = product.Id;
            option.OptionType = Enum.Parse<OptionType>(optReq.OptionType, true);
            await _unitOfWork.ProductOptions.AddAsync(option);
        }

        // Replace pricing tiers
        _unitOfWork.PricingTiers.RemoveRange(product.PricingTiers);
        foreach (var tierReq in request.PricingTiers)
        {
            var tier = _mapper.Map<PricingTier>(tierReq);
            tier.ProductId = product.Id;
            await _unitOfWork.PricingTiers.AddAsync(tier);
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
        return ApiResult<ProductDto>.Ok(_mapper.Map<ProductDto>(updated!), "Product updated.");
    }

    public async Task<ApiResult<bool>> DeactivateProductAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        product.IsActive = false;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return ApiResult<bool>.Ok(true, "Product deactivated.");
    }
}