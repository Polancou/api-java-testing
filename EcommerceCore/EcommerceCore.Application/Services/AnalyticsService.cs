using EcommerceCore.Application.DTOs;
using EcommerceCore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCore.Application.Services;

public class AnalyticsService(IApplicationDbContext context) : IAnalyticsService
{
    public async Task<AnalyticsDto> GetDashboardDataAsync()
    {
        var totalRevenue = await context.Orders.SumAsync(o => o.TotalAmount);
        var totalOrders = await context.Orders.CountAsync();
        var totalProducts = await context.Products.CountAsync();
        var totalUsers = await context.Usuarios.CountAsync();

        // Top 5 selling products
        var topProducts = await context.OrderItems
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToListAsync();

        // Monthly sales (last 6 months)
        // Note: Grouping by date in EF Core can be tricky depending on provider.
        // Assuming SQL Server, we can use client-side evaluation for small datasets or raw SQL.
        // For simplicity and compatibility, we'll fetch orders from last 6 months and group in memory.
        
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var recentOrders = await context.Orders
            .Where(o => o.OrderDate >= sixMonthsAgo)
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync();

        var monthlySales = recentOrders
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new MonthlySalesDto
            {
                Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(m => DateTime.Parse(m.Month))
            .ToList();

        return new AnalyticsDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalUsers = totalUsers,
            TopSellingProducts = topProducts,
            MonthlySales = monthlySales
        };
    }
}
