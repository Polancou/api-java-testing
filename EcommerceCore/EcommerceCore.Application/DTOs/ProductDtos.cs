namespace EcommerceCore.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
}

public class CreateProductDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
}
