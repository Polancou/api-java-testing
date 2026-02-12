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
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// Agrega una reseña a un producto.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> AddReview(CreateReviewDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        try
        {
            var review = await reviewService.AddReviewAsync(userId, dto);
            return CreatedAtAction(nameof(GetProductReviews), new { productId = dto.ProductId }, review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene las reseñas de un producto.
    /// </summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
    {
        var reviews = await reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    /// <summary>
    /// Elimina una reseña.
    /// </summary>
    [Authorize]
    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isAdmin = User.IsInRole("Admin");

        try
        {
            await reviewService.DeleteReviewAsync(reviewId, userId, isAdmin);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
