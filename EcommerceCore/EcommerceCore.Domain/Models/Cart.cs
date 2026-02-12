using System.ComponentModel.DataAnnotations;

namespace EcommerceCore.Domain.Models;

/// <summary>
/// Representa el carrito de compras de un usuario.
/// </summary>
public class Cart
{
    /// <summary>
    /// Identificador único del carrito.
    /// </summary>
    [Key] public int Id { get; private set; }

    /// <summary>
    /// Identificador del usuario al que pertenece el carrito.
    /// </summary>
    [Required] public int UserId { get; private set; }
    /// <summary>
    /// Objeto de navegación para el usuario propietario del carrito.
    /// </summary>
    public Usuario User { get; private set; }

    /// <summary>
    /// Lista de ítems contenidos en el carrito.
    /// </summary>
    public List<CartItem> Items { get; private set; } = new();

    /// <summary>
    /// Fecha de la última actualización del carrito.
    /// </summary>
    public DateTime LastUpdated { get; private set; }

    /// <summary>
    /// Constructor privado para uso de ORM.
    /// </summary>
    private Cart()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="Cart"/> con el ID del usuario especificado.
    /// </summary>
    /// <param name="userId">El ID del usuario al que pertenece este carrito.</param>
    public Cart(int userId)
    {
        UserId = userId;
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Agrega un producto al carrito o actualiza su cantidad si ya existe.
    /// </summary>
    /// <param name="productId">El ID del producto a agregar.</param>
    /// <param name="quantity">La cantidad del producto a agregar.</param>
    public void AddItem(int productId, int quantity)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Items.Add(new CartItem(productId,
                quantity));
        }

        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Elimina un producto del carrito.
    /// </summary>
    /// <param name="productId">El ID del producto a eliminar.</param>
    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }

        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Vacía el carrito por completo.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
        LastUpdated = DateTime.UtcNow;
    }
}

/// <summary>
/// Representa un ítem dentro del carrito de compras.
/// </summary>
public class CartItem
{
    /// <summary>
    /// Identificador único del ítem en el carrito.
    /// </summary>
    [Key] public int Id { get; private set; }

    /// <summary>
    /// Clave foránea al carrito al que pertenece este ítem.
    /// </summary>
    public int CartId { get; private set; }
    /// <summary>
    /// Objeto de navegación para el carrito al que pertenece este ítem.
    /// </summary>
    public Cart Cart { get; private set; }

    /// <summary>
    /// Clave foránea al producto que representa este ítem.
    /// </summary>
    public int ProductId { get; private set; }
    /// <summary>
    /// Objeto de navegación para el producto que representa este ítem.
    /// </summary>
    public Product Product { get; private set; }

    /// <summary>
    /// Cantidad del producto en el carrito.
    /// </summary>
    [Required] public int Quantity { get; private set; }

    /// <summary>
    /// Constructor privado requerido por Entity Framework.
    /// </summary>
    private CartItem()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="CartItem"/>.
    /// </summary>
    /// <param name="productId">El ID del producto a agregar.</param>
    /// <param name="quantity">La cantidad inicial del producto.</param>
    public CartItem(int productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    /// <summary>
    /// Actualiza la cantidad de este ítem en el carrito.
    /// </summary>
    /// <param name="quantity">La nueva cantidad del producto.</param>
    public void UpdateQuantity(int quantity)
    {
        // Asegura que la cantidad no sea negativa.
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity),
                "Quantity cannot be negative.");
        }
        Quantity = quantity;
    }
}
