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
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(i => i.Subtotal)
                .HasPrecision(18, 2);

            builder.Property(i => i.ConfigJson)
                .HasColumnType("jsonb");

            builder.Property(i => i.UploadFileUrl)
                .HasMaxLength(500);
        }
    }
}
