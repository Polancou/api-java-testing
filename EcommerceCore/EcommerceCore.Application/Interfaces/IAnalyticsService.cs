using EcommerceCore.Application.DTOs;

namespace EcommerceCore.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetDashboardDataAsync();
}
