using AutoMapper;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using EcommerceCore.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class ReviewService(IApplicationDbContext context, IMapper mapper) : IReviewService
{
    public async Task<ReviewDto> AddReviewAsync(int userId, CreateReviewDto dto)
    {
        // Check if user already reviewed this product
        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

        if (existingReview != null)
        {
            throw new InvalidOperationException("Ya has publicado una reseña para este producto.");
        }

        // Verify product exists
        var productExists = await context.Products.AnyAsync(p => p.Id == dto.ProductId);
        if (!productExists)
        {
            throw new KeyNotFoundException("Producto no encontrado.");
        }

        var review = new Review(dto.ProductId, userId, dto.Rating, dto.Comment);
        
        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        // Load user for mapping
        await context.Reviews.Entry(review).Reference(r => r.User).LoadAsync();

        return mapper.Map<ReviewDto>(review);
    }

    public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId)
    {
        var reviews = await context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }

    public async Task DeleteReviewAsync(int reviewId, int userId, bool isAdmin)
    {
        var review = await context.Reviews.FindAsync(reviewId);
        if (review == null) throw new KeyNotFoundException("Reseña no encontrada.");

        if (review.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("No tienes permiso para eliminar esta reseña.");
        }

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
    }

    public async Task<double> GetProductAverageRatingAsync(int productId)
    {
        var ratings = await context.Reviews
            .Where(r => r.ProductId == productId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (!ratings.Any()) return 0;

        return ratings.Average();
    }
}
