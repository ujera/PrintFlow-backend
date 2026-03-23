using PrintFlow.Domain.Common;
using PrintFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
