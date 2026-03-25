namespace PrintFlow.Application.DTOs.Cart;

// ── Responses ──

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int Quantity { get; set; }
    public string? ConfigJson { get; set; }
    public string? UploadFileUrl { get; set; }
    public decimal Subtotal { get; set; }
}

// ── Requests ──

public class AddCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? ConfigJson { get; set; }
    public string? UploadFileUrl { get; set; }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
    public string? ConfigJson { get; set; }
}