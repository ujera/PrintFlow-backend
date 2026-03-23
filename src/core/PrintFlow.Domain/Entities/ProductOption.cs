using PrintFlow.Domain.Common;
using PrintFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Domain.Entities
{
    public class ProductOption : BaseEntity
    {
        public Guid ProductId { get; set; }
        public OptionType OptionType { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceModifier { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
    }

}
