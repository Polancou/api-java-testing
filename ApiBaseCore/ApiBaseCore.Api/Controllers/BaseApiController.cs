using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ApiBaseCore.Api.Controllers;

[ApiController]
[Route(template: "api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the authenticated user's ID from the claims.
    /// Returns Guid.Empty if the user is not authenticated or the claim is missing.
    /// </summary>
    protected Guid UserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }
    }
}
