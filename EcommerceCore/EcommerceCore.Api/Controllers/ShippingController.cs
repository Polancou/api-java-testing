using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiVersion(version: "1.0")]
public class ShippingController(IShippingService shippingService) : BaseApiController
{
    /// <summary>
    /// Obtiene todas las direcciones de envío asociadas al usuario autenticado.
    /// </summary>
    /// <returns>Una lista de objetos <see cref="ShippingAddressDto"/> que representan las direcciones de envío del usuario.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var addresses = await shippingService.GetAddressesAsync(userId: UserId);
        return Ok(value: addresses);
    }

    /// <summary>
    /// Agrega una nueva dirección de envío para el usuario autenticado.
    /// </summary>
    /// <param name="dto">El objeto <see cref="ShippingAddressDto"/> que contiene los datos de la nueva dirección.</param>
    /// <returns>La dirección de envío creada, incluyendo su ID.</returns>
    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] ShippingAddressDto dto)
    {
        var createdAddress = await shippingService.AddAddressAsync(userId: UserId,
            dto: dto);
        // Devuelve un código 201 Created y la dirección creada.
        return CreatedAtAction(actionName: nameof(GetAddresses),
            value: createdAddress);
    }

    /// <summary>
    /// Elimina una dirección de envío específica para el usuario autenticado.
    /// </summary>
    /// <param name="id">El ID de la dirección de envío a eliminar.</param>
    /// <returns>Un <see cref="IActionResult"/> indicando el éxito de la operación (NoContent).</returns>
    [HttpDelete(template: "{id}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        await shippingService.DeleteAddressAsync(userId: UserId,
            addressId: id);
        // Devuelve un código 204 No Content para indicar que la eliminación fue exitosa.
        return NoContent();
    }
}
