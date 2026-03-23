using PrintFlow.Domain.Common;
using PrintFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class OrderStatusHistory : BaseEntity
    {
        public Guid OrderId { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public Guid ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public User ChangedBy { get; set; } = null!;
    }
}
