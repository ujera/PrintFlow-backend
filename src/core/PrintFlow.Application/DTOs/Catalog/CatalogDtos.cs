namespace PrintFlow.Application.DTOs.Catalog;

// ── Category Responses ──

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

// ── Category Requests ──

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

// ── Product Responses ──

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public List<ProductOptionDto> Options { get; set; } = new();
    public List<PricingTierDto> PricingTiers { get; set; } = new();
}

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
}

public class ProductOptionDto
{
    public Guid Id { get; set; }
    public string OptionType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; }
}

public class PricingTierDto
{
    public Guid Id { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// ── Product Requests ──

public class CreateProductRequest
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public List<CreateProductOptionRequest> Options { get; set; } = new();
    public List<CreatePricingTierRequest> PricingTiers { get; set; } = new();
}

public class UpdateProductRequest
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public List<CreateProductOptionRequest> Options { get; set; } = new();
    public List<CreatePricingTierRequest> PricingTiers { get; set; } = new();
}

public class CreateProductOptionRequest
{
    public string OptionType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; }
}

public class CreatePricingTierRequest
{
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}