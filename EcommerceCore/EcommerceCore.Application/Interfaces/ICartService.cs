using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface ICartService
{
    /// <summary>
    /// Obtiene el carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Un objeto CartDto que representa el carrito del usuario.</returns>
    Task<CartDto> GetCartAsync(int userId);

    /// <summary>
    /// Agrega un artículo al carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="dto">Los detalles del artículo a agregar.</param>
    /// <returns>Un objeto CartDto actualizado.</returns>
    Task<CartDto> AddItemAsync(int userId, AddToCartDto dto);

    /// <summary>
    /// Actualiza la cantidad de un artículo específico en el carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="productId">El ID del producto cuya cantidad se actualizará.</param>
    /// <param name="quantity">La nueva cantidad del producto.</param>
    /// <returns>Un objeto CartDto actualizado.</returns>
    Task<CartDto> UpdateItemQuantityAsync(int userId, int productId, int quantity);

    /// <summary>
    /// Elimina un artículo específico del carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="productId">El ID del producto a eliminar.</param>
    /// <returns>Un objeto CartDto actualizado.</returns>
    Task<CartDto> RemoveItemAsync(int userId, int productId);

    /// <summary>
    /// Sincroniza el carrito de compras de un usuario con una lista de artículos locales.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <param name="localItems">La lista de artículos locales para sincronizar.</param>
    /// <returns>Un objeto CartDto sincronizado.</returns>
    Task<CartDto> SyncCartAsync(int userId, List<AddToCartDto> localItems);

    /// <summary>
    /// Vacía completamente el carrito de compras de un usuario.
    /// </summary>
    /// <param name="userId">El ID del usuario.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    Task ClearCartAsync(int userId);
}
