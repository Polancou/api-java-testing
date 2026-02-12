using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[Authorize]
[ApiVersion(version: "1.0")]
public class CartController(ICartService cartService) : BaseApiController
{
    /// <summary>
    /// Obtiene el carrito actual del usuario.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var cart = await cartService.GetCartAsync(userId: UserId);
        return Ok(value: cart);
    }

    /// <summary>
    /// Sincroniza el carrito local (frontend) con el carrito del servidor.
    /// </summary>
    [HttpPost(template: "sync")]
    public async Task<IActionResult> SyncCart([FromBody] List<AddToCartDto> localItems)
    {
        var cart = await cartService.SyncCartAsync(userId: UserId,
            localItems: localItems);
        return Ok(value: cart);
    }

    /// <summary>
    /// Agrega un ítem al carrito.
    /// </summary>
    [HttpPost(template: "items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto itemDto)
    {
        var cart = await cartService.AddItemAsync(userId: UserId,
            dto: itemDto);
        return Ok(value: cart);
    }

    /// <summary>
    /// Actualiza la cantidad de un ítem en el carrito.
    /// </summary>
    [HttpPut(template: "items")]
    public async Task<IActionResult> UpdateItem([FromBody] AddToCartDto itemDto)
    {
        try
        {
            var cart = await cartService.UpdateItemQuantityAsync(userId: UserId,
                productId: itemDto.ProductId,
                quantity: itemDto.Quantity);
            return Ok(value: cart);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Elimina un ítem del carrito.
    /// </summary>
    [HttpDelete(template: "items/{productId}")]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        try
        {
            var cart = await cartService.RemoveItemAsync(userId: UserId,
                productId: productId);
            return Ok(value: cart);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
