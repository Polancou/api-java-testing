using Asp.Versioning;
using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    /// <summary>
    /// Obtiene datos para el dashboard de análisis.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AnalyticsDto>> GetDashboardData()
    {
        var data = await analyticsService.GetDashboardDataAsync();
        return Ok(data);
    }
}
