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
    public class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
    {
        public void Configure(EntityTypeBuilder<ProductOption> builder)
        {
            builder.ToTable("product_options");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OptionType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.PriceModifier)
                .HasPrecision(18, 2);
        }
    }
}
