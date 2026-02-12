using AutoMapper;
using AutoMapper.QueryableExtensions;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class ShippingService(IApplicationDbContext context, IMapper mapper) : IShippingService
{
    /// <summary>
    /// Obtiene todas las direcciones de envío asociadas a un usuario específico.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una colección de objetos ShippingAddressDto que representan las direcciones de envío del usuario.</returns>
    public async Task<IEnumerable<ShippingAddressDto>> GetAddressesAsync(int userId)
    {
        return await context.ShippingAddresses
            .Where(predicate: a => a.UserId == userId) // Filtra las direcciones por el ID del usuario
            .ProjectTo<ShippingAddressDto>(configuration: mapper.ConfigurationProvider) // Proyecta las entidades a DTOs
            .ToListAsync(); // Ejecuta la consulta y devuelve una lista
    }

    /// <summary>
    /// Agrega una nueva dirección de envío para un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario al que se le agregará la dirección.</param>
    /// <param name="dto">El objeto ShippingAddressDto que contiene los detalles de la nueva dirección.</param>
    /// <returns>El objeto ShippingAddressDto de la dirección recién agregada.</returns>
    public async Task<ShippingAddressDto> AddAddressAsync(int userId, ShippingAddressDto dto)
    {
        // Crea una nueva entidad ShippingAddress a partir del DTO y el userId
        var address = new ShippingAddress(
            userId: userId,
            name: dto.Name,
            addressLine1: dto.AddressLine1,
            city: dto.City,
            state: dto.State,
            postalCode: dto.PostalCode,
            country: dto.Country,
            addressLine2: dto.AddressLine2
        );

        context.ShippingAddresses.Add(entity: address); // Agrega la nueva dirección al contexto
        await context.SaveChangesAsync(); // Guarda los cambios en la base de datos

        return mapper.Map<ShippingAddressDto>(source: address); // Mapea la entidad guardada de nuevo a un DTO y la devuelve
    }

    /// <summary>
    /// Elimina una dirección de envío específica para un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario propietario de la dirección.</param>
    /// <param name="addressId">El ID de la dirección de envío a eliminar.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    public async Task DeleteAddressAsync(int userId, int addressId)
    {
        // Busca la dirección por su ID y el ID del usuario para asegurar que el usuario es el propietario
        var address = await context.ShippingAddresses
            .FirstOrDefaultAsync(predicate: a => a.Id == addressId && a.UserId == userId);

        if (address != null) // Si la dirección existe y pertenece al usuario
        {
            context.ShippingAddresses.Remove(entity: address); // Marca la dirección para ser eliminada
            await context.SaveChangesAsync(); // Guarda los cambios en la base de datos
        }
    }
}
