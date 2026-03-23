using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Persistence.Configurations
{
    public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
    {
        public void Configure(EntityTypeBuilder<PricingTier> builder)
        {
            builder.ToTable("pricing_tiers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.MinQuantity)
                .IsRequired();

            builder.Property(t => t.MaxQuantity)
                .IsRequired();

            builder.Property(t => t.UnitPrice)
                .HasPrecision(18, 2);

            builder.HasIndex(t => new { t.ProductId, t.MinQuantity })
                .IsUnique();
        }
    }
}
