using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

public class Product
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    [Key]
    public int Id { get; private set; }

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; private set; }

    /// <summary>
    /// Descripción detallada del producto.
    /// </summary>
    [Required]
    public string Description { get; private set; }

    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; private set; }

    /// <summary>
    /// Cantidad disponible en inventario.
    /// </summary>
    [Required]
    public int Stock { get; private set; }

    /// <summary>
    /// URL de la imagen del producto.
    /// </summary>
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// Categoría a la que pertenece el producto.
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    private Product()
    {
    }

    public Product(string name, string description, decimal price, int stock, string? imageUrl = null,
        string? category = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name),
            "El nombre es obligatorio.");
        if (price < 0) throw new ArgumentException("El precio no puede ser negativo.",
            nameof(price));
        if (stock < 0) throw new ArgumentException("El stock no puede ser negativo.",
            nameof(stock));

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        ImageUrl = imageUrl;
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el stock del producto.
    /// </summary>
    /// <param name="quantity">Cantidad a sumar (o restar si es negativa).</param>
    public void UpdateStock(int quantity)
    {
        if (Stock + quantity < 0) throw new InvalidOperationException("Stock insuficiente.");
        Stock += quantity;
    }

    /// <summary>
    /// Actualiza los detalles del producto.
    /// </summary>
    public void UpdateDetails(string name, string description, decimal price, string? imageUrl, string? category)
    {
        Name = name;
        Description = description;
        Price = price;
        ImageUrl = imageUrl;
        Category = category;
    }
}
