using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

/// <summary>
/// Representa los posibles estados de un pedido.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// El pedido ha sido creado pero aún no ha sido pagado.
    /// </summary>
    Pending,
    /// <summary>
    /// El pedido ha sido pagado exitosamente.
    /// </summary>
    Paid,
    /// <summary>
    /// El pedido ha sido enviado al cliente.
    /// </summary>
    Shipped,
    /// <summary>
    /// El pedido ha sido entregado al cliente.
    /// </summary>
    Delivered,
    /// <summary>
    /// El pedido ha sido cancelado.
    /// </summary>
    Cancelled
}

/// <summary>
/// Representa un pedido realizado por un usuario.
/// </summary>
public class Order
{
    /// <summary>
    /// Identificador único del pedido.
    /// </summary>
    [Key] public int Id { get; private set; }

    /// <summary>
    /// Identificador del usuario que realizó el pedido.
    /// </summary>
    [Required] public int UserId { get; private set; }
    /// <summary>
    /// Objeto de navegación para el usuario que realizó el pedido.
    /// </summary>
    public Usuario User { get; private set; }

    /// <summary>
    /// Fecha en la que se realizó el pedido.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Monto total del pedido.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Estado actual del pedido.
    /// </summary>
    [Required]
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// ID del intento de pago (por ejemplo, de un proveedor como Stripe). Puede ser nulo si el pago aún no se ha procesado.
    /// </summary>
    public string? PaymentIntentId { get; private set; }

    // Shipping Address Snapshot
    /// <summary>
    /// Primera línea de la dirección de envío.
    /// </summary>
    [Required] [MaxLength(100)] public string ShippingAddressLine1 { get; private set; } = string.Empty;
    /// <summary>
    /// Segunda línea opcional de la dirección de envío.
    /// </summary>
    [MaxLength(100)] public string? ShippingAddressLine2 { get; private set; }
    /// <summary>
    /// Ciudad de la dirección de envío.
    /// </summary>
    [Required] [MaxLength(50)] public string ShippingCity { get; private set; } = string.Empty;
    /// <summary>
    /// Estado o provincia de la dirección de envío.
    /// </summary>
    [Required] [MaxLength(50)] public string ShippingState { get; private set; } = string.Empty;
    /// <summary>
    /// Código postal de la dirección de envío.
    /// </summary>
    [Required] [MaxLength(20)] public string ShippingPostalCode { get; private set; } = string.Empty;
    /// <summary>
    /// País de la dirección de envío.
    /// </summary>
    [Required] [MaxLength(50)] public string ShippingCountry { get; private set; } = string.Empty;

    /// <summary>
    /// Lista de ítems incluidos en este pedido.
    /// </summary>
    public List<OrderItem> Items { get; private set; } = new();

    /// <summary>
    /// Constructor privado requerido por Entity Framework Core.
    /// </summary>
    private Order()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="Order"/>.
    /// </summary>
    /// <param name="userId">El ID del usuario que realiza el pedido.</param>
    /// <param name="totalAmount">El monto total del pedido.</param>
    public Order(int userId, decimal totalAmount)
    {
        UserId = userId;
        TotalAmount = totalAmount;
        OrderDate = DateTime.UtcNow; // Establece la fecha del pedido a la hora UTC actual.
        Status = OrderStatus.Pending; // Un nuevo pedido siempre comienza en estado Pendiente.
    }

    /// <summary>
    /// Agrega un ítem al pedido.
    /// </summary>
    /// <param name="item">El ítem de pedido a agregar.</param>
    public void AddItem(OrderItem item)
    {
        Items.Add(item);
    }

    /// <summary>
    /// Establece el ID del intento de pago para este pedido.
    /// </summary>
    /// <param name="paymentIntentId">El ID del intento de pago.</param>
    public void SetPaymentIntentId(string paymentIntentId)
    {
        PaymentIntentId = paymentIntentId;
    }

    /// <summary>
    /// Establece la dirección de envío para el pedido.
    /// </summary>
    /// <param name="addressLine1">Primera línea de la dirección.</param>
    /// <param name="addressLine2">Segunda línea opcional de la dirección.</param>
    /// <param name="city">Ciudad.</param>
    /// <param name="state">Estado o provincia.</param>
    /// <param name="postalCode">Código postal.</param>
    /// <param name="country">País.</param>
    public void SetShippingAddress(string addressLine1, string? addressLine2, string city, string state,
        string postalCode, string country)
    {
        ShippingAddressLine1 = addressLine1;
        ShippingAddressLine2 = addressLine2;
        ShippingCity = city;
        ShippingState = state;
        ShippingPostalCode = postalCode;
        ShippingCountry = country;
    }

    /// <summary>
    /// Marca el pedido como pagado.
    /// </summary>
    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
    }

    /// <summary>
    /// Actualiza el estado del pedido a un nuevo estado.
    /// </summary>
    /// <param name="newStatus">El nuevo estado del pedido.</param>
    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}

/// <summary>
/// Representa un ítem dentro de un pedido.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Identificador único del ítem del pedido.
    /// </summary>
    [Key] public int Id { get; private set; }

    /// <summary>
    /// Identificador del pedido al que pertenece este ítem.
    /// </summary>
    public int OrderId { get; private set; }
    /// <summary>
    /// Objeto de navegación para el pedido al que pertenece este ítem.
    /// </summary>
    public Order Order { get; private set; }

    /// <summary>
    /// Identificador del producto asociado a este ítem.
    /// </summary>
    public int ProductId { get; private set; }
    /// <summary>
    /// Objeto de navegación para el producto asociado a este ítem.
    /// </summary>
    public Product Product { get; private set; }

    /// <summary>
    /// Nombre del producto en el momento de la compra (instantánea).
    /// </summary>
    [Required] public string ProductName { get; private set; }

    /// <summary>
    /// Precio unitario del producto en el momento de la compra (instantánea).
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Cantidad del producto en este ítem del pedido.
    /// </summary>
    [Required] public int Quantity { get; private set; }

    /// <summary>
    /// Constructor privado requerido por Entity Framework Core.
    /// </summary>
    private OrderItem()
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="OrderItem"/>.
    /// </summary>
    /// <param name="productId">El ID del producto.</param>
    /// <param name="productName">El nombre del producto.</param>
    /// <param name="unitPrice">El precio unitario del producto.</param>
    /// <param name="quantity">La cantidad del producto.</param>
    public OrderItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
}
