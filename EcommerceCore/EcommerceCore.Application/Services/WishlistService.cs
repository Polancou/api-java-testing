using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class WishlistService(IApplicationDbContext context, IMapper mapper) : IWishlistService
{
    public async Task AddToWishlistAsync(int userId, int productId)
    {
        var exists = await context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

        if (exists) return; // Already in wishlist

        var productExists = await context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists) throw new KeyNotFoundException("Producto no encontrado.");

        var item = new WishlistItem(userId, productId);
        context.WishlistItems.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task RemoveFromWishlistAsync(int userId, int productId)
    {
        var item = await context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (item != null)
        {
            context.WishlistItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<WishlistItemDto>> GetUserWishlistAsync(int userId)
    {
        var items = await context.WishlistItems
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();

        return mapper.Map<IEnumerable<WishlistItemDto>>(items);
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
    {
        return await context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }
}
