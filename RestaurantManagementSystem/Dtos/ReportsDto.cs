using System.Collections.Generic;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Dtos
{
    public class DailySalesReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrdersCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<HourlySalesDto> HourlySales { get; set; } = new();
    }

    public class HourlySalesDto
    {
        public int Hour { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class PopularItemDto
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DashboardSummaryDto
    {
        public decimal DailyRevenue { get; set; }
        public int DailyOrdersCount { get; set; }
        public int ActiveTablesCount { get; set; }
        public int LowStockItemsCount { get; set; }
        public List<PopularItemDto> PopularItems { get; set; } = new();
        public List<InventoryItem> LowStockAlerts { get; set; } = new();
    }
}
