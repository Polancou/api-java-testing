using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceCore.Domain.Models;

public class WishlistItem
{
    [Key] public int Id { get; private set; }

    [Required] public int UserId { get; private set; }

    [Required] public int ProductId { get; private set; }

    public DateTime AddedAt { get; private set; }

    // Navigation properties
    [ForeignKey("UserId")] public virtual Usuario User { get; private set; }

    [ForeignKey("ProductId")] public virtual Product Product { get; private set; }

    private WishlistItem()
    {
        User = null!;
        Product = null!;
    }

    public WishlistItem(int userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
        AddedAt = DateTime.UtcNow;
    }
}
