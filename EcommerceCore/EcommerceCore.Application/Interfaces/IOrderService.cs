using EcommerceCore.Application.DTOs;
using EcommerceCore.Domain.Models;

namespace EcommerceCore.Application.Interfaces;

public interface IOrderService
{
    /// <summary>
    /// Obtiene todos los pedidos de un usuario específico de forma asíncrona.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una tarea que representa la operación asíncrona, que devuelve una colección de DTOs de pedidos.</returns>
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);

    /// <summary>
    /// Obtiene todos los pedidos del sistema de forma asíncrona (función de administrador).
    /// </summary>
    /// <returns>Una tarea que representa la operación asíncrona, que devuelve una colección de DTOs de pedidos.</returns>
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(); 

    /// <summary>
    /// Obtiene un pedido por su ID de forma asíncrona.
    /// </summary>
    /// <param name="id">El ID del pedido.</param>
    /// <param name="userId">El ID del usuario que solicita el pedido (para validación).</param>
    /// <param name="isAdmin">Indica si el solicitante es un administrador.</param>
    /// <returns>Una tarea que representa la operación asíncrona, que devuelve el DTO del pedido si se encuentra, o nulo si no.</returns>
    Task<OrderDto?> GetOrderByIdAsync(int id, int userId, bool isAdmin);

    /// <summary>
    /// Crea un nuevo pedido de forma asíncrona.
    /// </summary>
    /// <param name="userId">El ID del usuario que realiza el pedido.</param>
    /// <param name="shippingAddress">El DTO de la dirección de envío para el pedido.</param>
    /// <returns>Una tarea que representa la operación asíncrona, que devuelve el DTO del pedido creado.</returns>
    Task<OrderDto> CreateOrderAsync(int userId, ShippingAddressDto shippingAddress);

    /// <summary>
    /// Actualiza el estado de un pedido específico de forma asíncrona.
    /// </summary>
    /// <param name="orderId">El ID del pedido a actualizar.</param>
    /// <param name="newStatus">El nuevo estado del pedido.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
}
