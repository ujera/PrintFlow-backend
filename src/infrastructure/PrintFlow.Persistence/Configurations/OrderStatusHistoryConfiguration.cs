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
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("order_status_history");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.OldStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(h => h.NewStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(h => h.ChangedAt)
                .IsRequired();

            builder.Property(h => h.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(h => new { h.OrderId, h.ChangedAt });
        }
    }
}
