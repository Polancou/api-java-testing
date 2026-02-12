using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[ApiController]
[Route(template: "api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Gets the authenticated user's ID from the claims.
    /// Returns 0 if the user is not authenticated or the claim is missing.
    /// </summary>
    protected int UserId
    {
        get
        {
            var userIdClaim = User.FindFirst(type: ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(s: userIdClaim.Value,
                result: out var userId) ? userId : 0;
        }
    }
}
