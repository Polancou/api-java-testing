using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceCore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class WishlistController(IWishlistService wishlistService) : ControllerBase
{
    /// <summary>
    /// Agrega un producto a la lista de deseos.
    /// </summary>
    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToWishlist(int productId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            await wishlistService.AddToWishlistAsync(userId, productId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Producto no encontrado.");
        }
    }

    /// <summary>
    /// Elimina un producto de la lista de deseos.
    /// </summary>
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromWishlist(int productId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await wishlistService.RemoveFromWishlistAsync(userId, productId);
        return NoContent();
    }

    /// <summary>
    /// Obtiene la lista de deseos del usuario actual.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var items = await wishlistService.GetUserWishlistAsync(userId);
        return Ok(items);
    }

    /// <summary>
    /// Verifica si un producto está en la lista de deseos.
    /// </summary>
    [HttpGet("check/{productId}")]
    public async Task<ActionResult<bool>> CheckWishlist(int productId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var exists = await wishlistService.IsInWishlistAsync(userId, productId);
        return Ok(exists);
    }
}
