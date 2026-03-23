using PrintFlow.Domain.Common;
using PrintFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? StripePaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
    }
}
