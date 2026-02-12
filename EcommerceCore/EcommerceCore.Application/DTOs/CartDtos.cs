namespace EcommerceCore.Application.DTOs;

public class CartDto
{
    public int Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
}

public class CartItemDto
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}

public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
