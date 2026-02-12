using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

public class Review
{
    [Key] public int Id { get; private set; }

    [Required] public int ProductId { get; private set; }

    [Required] public int UserId { get; private set; }

    [Required] [Range(1, 5)] public int Rating { get; private set; }

    [Required] [MaxLength(1000)] public string Comment { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    [ForeignKey("ProductId")] public virtual Product Product { get; private set; }

    [ForeignKey("UserId")] public virtual Usuario User { get; private set; }

    private Review()
    {
        Comment = null!;
        Product = null!;
        User = null!;
    }

    public Review(int productId, int userId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        if (string.IsNullOrWhiteSpace(comment))
            throw new ArgumentNullException(nameof(comment), "Comment cannot be empty.");

        ProductId = productId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}
