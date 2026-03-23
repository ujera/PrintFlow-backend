using PrintFlow.Domain.Common;

namespace PrintFlow.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? ConfigJson { get; set; }     // Selected options serialized
        public string? UploadFileUrl { get; set; }  // Customer's uploaded design

        // Navigation properties
        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
