using PrintFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ProductCategory Category { get; set; } = null!;
        public ICollection<ProductOption> Options { get; set; } = new List<ProductOption>();
        public ICollection<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
