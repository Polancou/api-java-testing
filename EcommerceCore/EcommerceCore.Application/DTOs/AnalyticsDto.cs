namespace EcommerceCore.Application.DTOs;

public class AnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public List<TopProductDto> TopSellingProducts { get; set; } = new();
    public List<MonthlySalesDto> MonthlySales { get; set; } = new();
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlySalesDto
{
    public string Month { get; set; } = string.Empty; // e.g., "Jan 2023"
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
