using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Persistence.Seed;

public static class PrintFlowSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedCategories(modelBuilder);
        SeedProducts(modelBuilder);
        SeedProductOptions(modelBuilder);
        SeedPricingTiers(modelBuilder);
    }

    // ── Fixed GUIDs for deterministic seeding ──
    public static readonly Guid AdminRoleId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid CustomerRoleId = Guid.Parse("a0000000-0000-0000-0000-000000000002");

    private static readonly Guid CatBusinessCards = Guid.Parse("b1000000-0000-0000-0000-000000000001");
    private static readonly Guid CatBanners = Guid.Parse("b1000000-0000-0000-0000-000000000002");
    private static readonly Guid CatApparel = Guid.Parse("b1000000-0000-0000-0000-000000000003");
    private static readonly Guid CatStickers = Guid.Parse("b1000000-0000-0000-0000-000000000004");

    private static readonly Guid ProdStandardCard = Guid.Parse("c1000000-0000-0000-0000-000000000001");
    private static readonly Guid ProdVinylBanner = Guid.Parse("c1000000-0000-0000-0000-000000000002");
    private static readonly Guid ProdTShirt = Guid.Parse("c1000000-0000-0000-0000-000000000003");
    private static readonly Guid ProdDieSticker = Guid.Parse("c1000000-0000-0000-0000-000000000004");

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = AdminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole<Guid>
            {
                Id = CustomerRoleId,
                Name = "Customer",
                NormalizedName = "CUSTOMER"
            }
        );
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategory>().HasData(
            new ProductCategory
            {
                Id = CatBusinessCards,
                Name = "Business Cards",
                Slug = "business-cards",
                Description = "Professional business cards with premium finishes",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductCategory
            {
                Id = CatBanners,
                Name = "Banners & Signs",
                Slug = "banners-signs",
                Description = "Large format banners, signage, and displays",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductCategory
            {
                Id = CatApparel,
                Name = "T-Shirts & Apparel",
                Slug = "t-shirts-apparel",
                Description = "Custom printed clothing and branded merchandise",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ProductCategory
            {
                Id = CatStickers,
                Name = "Stickers & Labels",
                Slug = "stickers-labels",
                Description = "Die-cut stickers, labels, and decals",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = ProdStandardCard,
                CategoryId = CatBusinessCards,
                Name = "Standard Business Card",
                Slug = "standard-business-card",
                Description = "3.5\" x 2\" full-color business cards on premium cardstock",
                BasePrice = 0.15m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = ProdVinylBanner,
                CategoryId = CatBanners,
                Name = "Vinyl Banner",
                Slug = "vinyl-banner",
                Description = "Durable outdoor vinyl banner with grommets",
                BasePrice = 25.00m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = ProdTShirt,
                CategoryId = CatApparel,
                Name = "Custom T-Shirt",
                Slug = "custom-t-shirt",
                Description = "DTG printed cotton t-shirt with your design",
                BasePrice = 12.00m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = ProdDieSticker,
                CategoryId = CatStickers,
                Name = "Die-Cut Sticker",
                Slug = "die-cut-sticker",
                Description = "Custom shaped vinyl stickers, weatherproof",
                BasePrice = 0.10m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static void SeedProductOptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductOption>().HasData(
            // Business Card options
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000001"), ProductId = ProdStandardCard, OptionType = OptionType.Material, Name = "Standard 300gsm", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000002"), ProductId = ProdStandardCard, OptionType = OptionType.Material, Name = "Premium 400gsm", PriceModifier = 0.05m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000003"), ProductId = ProdStandardCard, OptionType = OptionType.Finishing, Name = "Matte Lamination", PriceModifier = 0.03m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000004"), ProductId = ProdStandardCard, OptionType = OptionType.Finishing, Name = "Gloss Lamination", PriceModifier = 0.03m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000005"), ProductId = ProdStandardCard, OptionType = OptionType.Finishing, Name = "Foil Stamping", PriceModifier = 0.10m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000006"), ProductId = ProdStandardCard, OptionType = OptionType.Finishing, Name = "Rounded Corners", PriceModifier = 0.02m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Vinyl Banner options
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000010"), ProductId = ProdVinylBanner, OptionType = OptionType.Size, Name = "3ft x 6ft", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000011"), ProductId = ProdVinylBanner, OptionType = OptionType.Size, Name = "4ft x 8ft", PriceModifier = 15.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000012"), ProductId = ProdVinylBanner, OptionType = OptionType.Material, Name = "13oz Vinyl", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000013"), ProductId = ProdVinylBanner, OptionType = OptionType.Material, Name = "18oz Heavy Vinyl", PriceModifier = 10.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // T-Shirt options
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000020"), ProductId = ProdTShirt, OptionType = OptionType.Size, Name = "S", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000021"), ProductId = ProdTShirt, OptionType = OptionType.Size, Name = "M", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000022"), ProductId = ProdTShirt, OptionType = OptionType.Size, Name = "L", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000023"), ProductId = ProdTShirt, OptionType = OptionType.Size, Name = "XL", PriceModifier = 2.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000024"), ProductId = ProdTShirt, OptionType = OptionType.Material, Name = "Cotton", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000025"), ProductId = ProdTShirt, OptionType = OptionType.Material, Name = "Polyester Blend", PriceModifier = 3.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Die-Cut Sticker options
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000030"), ProductId = ProdDieSticker, OptionType = OptionType.Size, Name = "2\" x 2\"", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000031"), ProductId = ProdDieSticker, OptionType = OptionType.Size, Name = "3\" x 3\"", PriceModifier = 0.05m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000032"), ProductId = ProdDieSticker, OptionType = OptionType.Finishing, Name = "Glossy", PriceModifier = 0m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ProductOption { Id = Guid.Parse("d1000000-0000-0000-0000-000000000033"), ProductId = ProdDieSticker, OptionType = OptionType.Finishing, Name = "Matte", PriceModifier = 0.02m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }

    private static void SeedPricingTiers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PricingTier>().HasData(
            // Business Cards
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000001"), ProductId = ProdStandardCard, MinQuantity = 100, MaxQuantity = 249, UnitPrice = 0.15m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000002"), ProductId = ProdStandardCard, MinQuantity = 250, MaxQuantity = 499, UnitPrice = 0.12m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000003"), ProductId = ProdStandardCard, MinQuantity = 500, MaxQuantity = 999, UnitPrice = 0.09m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000004"), ProductId = ProdStandardCard, MinQuantity = 1000, MaxQuantity = 10000, UnitPrice = 0.06m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // T-Shirts
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000010"), ProductId = ProdTShirt, MinQuantity = 1, MaxQuantity = 9, UnitPrice = 12.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000011"), ProductId = ProdTShirt, MinQuantity = 10, MaxQuantity = 49, UnitPrice = 10.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000012"), ProductId = ProdTShirt, MinQuantity = 50, MaxQuantity = 500, UnitPrice = 8.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Die-Cut Stickers
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000020"), ProductId = ProdDieSticker, MinQuantity = 50, MaxQuantity = 199, UnitPrice = 0.10m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000021"), ProductId = ProdDieSticker, MinQuantity = 200, MaxQuantity = 499, UnitPrice = 0.07m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000022"), ProductId = ProdDieSticker, MinQuantity = 500, MaxQuantity = 5000, UnitPrice = 0.04m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Vinyl Banners
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000030"), ProductId = ProdVinylBanner, MinQuantity = 1, MaxQuantity = 4, UnitPrice = 25.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new PricingTier { Id = Guid.Parse("e1000000-0000-0000-0000-000000000031"), ProductId = ProdVinylBanner, MinQuantity = 5, MaxQuantity = 20, UnitPrice = 20.00m, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}