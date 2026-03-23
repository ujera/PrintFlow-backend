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
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("cart_items");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Quantity)
                .IsRequired();

            builder.Property(c => c.ConfigJson)
                .HasColumnType("jsonb");

            builder.Property(c => c.UploadFileUrl)
                .HasMaxLength(500);

            builder.HasIndex(c => c.UserId);
        }
    }
}
