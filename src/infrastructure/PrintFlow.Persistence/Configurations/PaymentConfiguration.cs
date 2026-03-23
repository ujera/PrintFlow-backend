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
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Method)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(p => p.StripePaymentId)
                .HasMaxLength(256);

            builder.HasIndex(p => p.StripePaymentId)
                .IsUnique()
                .HasFilter("\"StripePaymentId\" IS NOT NULL");

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}
