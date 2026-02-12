using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface IShippingService
{
    /// <summary>
    /// Obtiene todas las direcciones de envío asociadas a un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una colección de direcciones de envío.</returns>
    Task<IEnumerable<ShippingAddressDto>> GetAddressesAsync(int userId);
    /// <summary>
    /// Agrega una nueva dirección de envío para un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="dto">Los datos de la dirección de envío.</param>
    /// <returns>La dirección de envío recién agregada.</returns>
    Task<ShippingAddressDto> AddAddressAsync(int userId, ShippingAddressDto dto);
    /// <summary>
    /// Elimina una dirección de envío para un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="addressId">El ID de la dirección de envío a eliminar.</param>
    Task DeleteAddressAsync(int userId, int addressId);
}
