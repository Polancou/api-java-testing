using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiVersion(version: "1.0")]
public class OrdersController(IOrderService orderService) : BaseApiController
{
    /// <summary>
    /// Obtiene los pedidos del usuario actual o todos si es Admin.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        // Comprueba si el usuario actual tiene el rol de "Admin".
        var isAdmin = User.IsInRole(role: "Admin");

        // Si el usuario es administrador, recupera todos los pedidos.
        if (isAdmin)
        {
            return Ok(value: await orderService.GetAllOrdersAsync());
        }

        // Si no es administrador, recupera solo los pedidos asociados al ID del usuario actual.
        return Ok(value: await orderService.GetUserOrdersAsync(userId: UserId));
    }

    /// <summary>
    /// Obtiene un pedido específico por su ID.
    /// </summary>
    /// <param name="id">El ID del pedido a recuperar.</param>
    [HttpGet(template: "{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        // Comprueba si el usuario actual tiene el rol de "Admin".
        var isAdmin = User.IsInRole(role: "Admin");

        try
        {
            // Intenta obtener el pedido por su ID, verificando la autorización del usuario.
            var order = await orderService.GetOrderByIdAsync(id: id,
                userId: UserId,
                isAdmin: isAdmin);
            // Si el pedido no se encuentra, devuelve un 404 Not Found.
            if (order == null) return NotFound();
            // Devuelve el pedido encontrado.
            return Ok(value: order);
        }
        catch (UnauthorizedAccessException)
        {
            // Si el usuario no tiene permiso para acceder a este pedido, devuelve un 403 Forbid.
            return Forbid();
        }
    }

    /// <summary>
    /// Crea un nuevo pedido basado en el carrito actual del usuario.
    /// </summary>
    /// <param name="shippingAddress">Los detalles de la dirección de envío para el nuevo pedido.</param>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] ShippingAddressDto shippingAddress)
    {
        try
        {
            // Intenta crear un nuevo pedido utilizando el carrito del usuario y la dirección de envío proporcionada.
            var order = await orderService.CreateOrderAsync(userId: UserId,
                shippingAddress: shippingAddress);
            // Devuelve un 201 Created con la ubicación del nuevo recurso y el objeto del pedido.
            return CreatedAtAction(actionName: nameof(GetOrder),
                routeValues: new
                {
                    id = order.Id
                },
                value: order);
        }
        catch (InvalidOperationException ex)
        {
            // Si ocurre una operación inválida (ej. carrito vacío), devuelve un 400 Bad Request.
            return BadRequest(error: ex.Message);
        }
    }

    /// <summary>
    /// Actualiza el estado de un pedido (Requiere rol Admin).
    /// </summary>
    /// <param name="id">El ID del pedido a actualizar.</param>
    /// <param name="dto">El DTO que contiene el nuevo estado del pedido.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut(template: "{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            // Intenta actualizar el estado del pedido.
            await orderService.UpdateOrderStatusAsync(orderId: id,
                newStatus: dto.Status);
            // Si la actualización es exitosa, devuelve un 204 No Content.
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            // Si el pedido no se encuentra, devuelve un 404 Not Found.
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            // Si hay un argumento inválido (ej. estado no válido), devuelve un 400 Bad Request.
            return BadRequest(error: ex.Message);
        }
    }
}
