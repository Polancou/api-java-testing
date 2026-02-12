using System.ComponentModel.DataAnnotations;

namespace EcommerceCore.Application.DTOs;

public class CreateReviewDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
