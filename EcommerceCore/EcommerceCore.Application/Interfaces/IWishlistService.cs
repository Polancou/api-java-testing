using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface IWishlistService
{
    Task AddToWishlistAsync(int userId, int productId);
    Task RemoveFromWishlistAsync(int userId, int productId);
    Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
}
