using PrintFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class PricingTier : BaseEntity
    {
        public Guid ProductId { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
    }
}
