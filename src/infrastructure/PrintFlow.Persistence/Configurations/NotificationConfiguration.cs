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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(n => n.Subject)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(n => n.Body)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(n => n.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}
