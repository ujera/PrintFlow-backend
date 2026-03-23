using Microsoft.AspNetCore.Identity;
using PrintFlow.Domain.Common;
using PrintFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? GoogleId { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<OrderStatusHistory> StatusChanges { get; set; } = new List<OrderStatusHistory>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
