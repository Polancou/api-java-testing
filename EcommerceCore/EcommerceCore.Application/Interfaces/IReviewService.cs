using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> AddReviewAsync(int userId, CreateReviewDto dto);
    Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId);
    Task DeleteReviewAsync(int reviewId, int userId, bool isAdmin);
    Task<double> GetProductAverageRatingAsync(int productId);
}
