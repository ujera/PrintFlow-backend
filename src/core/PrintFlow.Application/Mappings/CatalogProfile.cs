using AutoMapper;
using PrintFlow.Application.DTOs.Catalog;
using PrintFlow.Domain.Entities;

namespace PrintFlow.Application.Mappings;

public class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        // ── Category ──
        CreateMap<ProductCategory, CategoryDto>()
            .ForMember(d => d.ProductCount, opt => opt.MapFrom(s => s.Products.Count));

        CreateMap<CreateCategoryRequest, ProductCategory>()
            .ForMember(d => d.Slug, opt => opt.Ignore())   // generated in service
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Products, opt => opt.Ignore());

        // ── Product ──
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s =>
                s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.Options, opt => opt.MapFrom(s => s.Options))
            .ForMember(d => d.PricingTiers, opt => opt.MapFrom(s => s.PricingTiers));

        CreateMap<Product, ProductListDto>();

        CreateMap<CreateProductRequest, Product>()
            .ForMember(d => d.Slug, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.Options, opt => opt.Ignore())
            .ForMember(d => d.PricingTiers, opt => opt.Ignore())
            .ForMember(d => d.OrderItems, opt => opt.Ignore());

        // ── Product Option ──
        CreateMap<ProductOption, ProductOptionDto>()
            .ForMember(d => d.OptionType, opt => opt.MapFrom(s => s.OptionType.ToString()));

        CreateMap<CreateProductOptionRequest, ProductOption>()
            .ForMember(d => d.OptionType, opt => opt.Ignore())  // parsed in service
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProductId, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());

        // ── Pricing Tier ──
        CreateMap<PricingTier, PricingTierDto>();

        CreateMap<CreatePricingTierRequest, PricingTier>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProductId, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore());
    }
}