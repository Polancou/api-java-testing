using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class CartService(IApplicationDbContext context, IMapper mapper) : ICartService
{
    /// <summary>
    /// Obtiene el carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Un objeto <see cref="CartDto"/> que representa el carrito del usuario, o un carrito vacío si no existe.</returns>
    public async Task<CartDto> GetCartAsync(int userId)
    {
        // Intenta obtener la entidad del carrito del usuario
        var cart = await GetCartEntityAsync(userId: userId);
        // Si el carrito no existe, devuelve un DTO de carrito vacío
        if (cart == null) return new CartDto { Id = 0, Items = new List<CartItemDto>() };

        // Mapea la entidad del carrito a un DTO y lo devuelve
        return mapper.Map<CartDto>(source: cart);
    }

    /// <summary>
    /// Agrega un producto al carrito de compras de un usuario o actualiza su cantidad si ya existe.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="dto">Un objeto <see cref="AddToCartDto"/> con los detalles del producto a agregar.</param>
    /// <returns>Un objeto <see cref="CartDto"/> actualizado del carrito del usuario.</returns>
    public async Task<CartDto> AddItemAsync(int userId, AddToCartDto dto)
    {
        // Intenta obtener la entidad del carrito del usuario
        var cart = await GetCartEntityAsync(userId: userId);
        // Si el carrito no existe, crea uno nuevo para el usuario
        if (cart == null)
        {
            cart = new Cart(userId: userId);
            context.Carts.Add(entity: cart); // Agrega el nuevo carrito al contexto
        }

        // Agrega el item al carrito (maneja la lógica de actualización si ya existe)
        cart.AddItem(productId: dto.ProductId,
            quantity: dto.Quantity);
        await context.SaveChangesAsync(); // Guarda los cambios en la base de datos

        // Vuelve a obtener el carrito para asegurar que los detalles del producto estén cargados para el DTO
        cart = await GetCartEntityAsync(userId: userId);
        return mapper.Map<CartDto>(source: cart!); // Mapea y devuelve el carrito actualizado
    }

    /// <summary>
    /// Actualiza la cantidad de un producto específico en el carrito de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="productId">El ID del producto a actualizar.</param>
    /// <param name="quantity">La nueva cantidad del producto.</param>
    /// <returns>Un objeto <see cref="CartDto"/> actualizado del carrito del usuario.</returns>
    /// <exception cref="KeyNotFoundException">Se lanza si el carrito del usuario no se encuentra.</exception>
    public async Task<CartDto> UpdateItemQuantityAsync(int userId, int productId, int quantity)
    {
        // Intenta obtener la entidad del carrito del usuario
        var cart = await GetCartEntityAsync(userId: userId);
        // Si el carrito no existe, lanza una excepción
        if (cart == null) throw new KeyNotFoundException(message: "Carrito no encontrado.");

        // Busca el item específico en el carrito
        var item = cart.Items.FirstOrDefault(predicate: i => i.ProductId == productId);
        if (item != null)
        {
            // Actualiza la cantidad del item
            item.UpdateQuantity(quantity: quantity);
            await context.SaveChangesAsync(); // Guarda los cambios
        }

        // Vuelve a obtener el carrito para asegurar el estado correcto para el DTO
        cart = await GetCartEntityAsync(userId: userId);
        return mapper.Map<CartDto>(source: cart!); // Mapea y devuelve el carrito actualizado
    }

    /// <summary>
    /// Elimina un producto específico del carrito de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="productId">El ID del producto a eliminar.</param>
    /// <returns>Un objeto <see cref="CartDto"/> actualizado del carrito del usuario.</returns>
    /// <exception cref="KeyNotFoundException">Se lanza si el carrito del usuario no se encuentra.</exception>
    public async Task<CartDto> RemoveItemAsync(int userId, int productId)
    {
        // Intenta obtener la entidad del carrito del usuario
        var cart = await GetCartEntityAsync(userId: userId);
        // Si el carrito no existe, lanza una excepción
        if (cart == null) throw new KeyNotFoundException(message: "Carrito no encontrado.");

        // Elimina el item del carrito
        cart.RemoveItem(productId: productId);
        await context.SaveChangesAsync(); // Guarda los cambios

        // Vuelve a obtener el carrito para asegurar el estado correcto después de la eliminación
        cart = await GetCartEntityAsync(userId: userId);
        return mapper.Map<CartDto>(source: cart!); // Mapea y devuelve el carrito actualizado
    }

    /// <summary>
    /// Sincroniza los items de un carrito local con el carrito del usuario en la base de datos.
    /// Agrega items nuevos o actualiza cantidades de existentes.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="localItems">Una lista de <see cref="AddToCartDto"/> que representan los items del carrito local.</param>
    /// <returns>Un objeto <see cref="CartDto"/> actualizado del carrito del usuario.</returns>
    public async Task<CartDto> SyncCartAsync(int userId, List<AddToCartDto> localItems)
    {
        // Intenta obtener la entidad del carrito del usuario
        var cart = await GetCartEntityAsync(userId: userId);
        // Si el carrito no existe, crea uno nuevo
        if (cart == null)
        {
            cart = new Cart(userId: userId);
            context.Carts.Add(entity: cart); // Agrega el nuevo carrito al contexto
        }

        // Itera sobre los items locales y los agrega al carrito del servidor
        // La lógica AddItem() maneja si el item ya existe (actualiza cantidad) o es nuevo
        foreach (var item in localItems)
        {
            cart.AddItem(productId: item.ProductId,
                quantity: item.Quantity);
        }

        await context.SaveChangesAsync(); // Guarda todos los cambios

        // Vuelve a obtener el carrito para asegurar que los detalles del producto estén cargados para el DTO
        cart = await GetCartEntityAsync(userId: userId);
        return mapper.Map<CartDto>(source: cart!); // Mapea y devuelve el carrito sincronizado
    }

    /// <summary>
    /// Vacía completamente el carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    public async Task ClearCartAsync(int userId)
    {
        // Intenta obtener la entidad del carrito del usuario
        var cart = await GetCartEntityAsync(userId: userId);
        if (cart != null)
        {
            // Vacía todos los items del carrito
            cart.Clear();
            await context.SaveChangesAsync(); // Guarda los cambios
        }
    }

    /// <summary>
    /// Obtiene la entidad del carrito de compras para un usuario específico, incluyendo sus items y los productos asociados.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una entidad <see cref="Cart"/> o null si no se encuentra.</returns>
    private async Task<Cart?> GetCartEntityAsync(int userId)
    {
        // Consulta la base de datos para obtener el carrito
        return await context.Carts
            .Include(navigationPropertyPath: c => c.Items) // Incluye los items del carrito
            .ThenInclude(navigationPropertyPath: i => i.Product) // Luego incluye los detalles del producto para cada item
            .FirstOrDefaultAsync(predicate: c => c.UserId == userId); // Busca el carrito por el ID de usuario
    }
}
