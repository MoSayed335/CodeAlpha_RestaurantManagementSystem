using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Dtos;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Services
{
    public class ReportingService : IReportingService
    {
        private readonly RestaurantDbContext _context;

        public ReportingService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<DailySalesReportDto> GetDailySalesReportAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var ordersToday = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderTime >= todayUtc && o.Status == OrderStatus.Paid)
                .ToListAsync();

            var totalRevenue = ordersToday.Sum(o => o.TotalAmount);
            var totalOrdersCount = ordersToday.Count;
            var avgOrderValue = totalOrdersCount > 0 ? totalRevenue / totalOrdersCount : 0;

            var hourlySales = ordersToday
                .GroupBy(o => o.OrderTime.Hour)
                .Select(g => new HourlySalesDto
                {
                    Hour = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToList();

            return new DailySalesReportDto
            {
                TotalRevenue = totalRevenue,
                TotalOrdersCount = totalOrdersCount,
                AverageOrderValue = avgOrderValue,
                HourlySales = hourlySales
            };
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;

            var paidOrdersToday = await _context.Orders
                .Where(o => o.OrderTime >= todayUtc && o.Status == OrderStatus.Paid)
                .ToListAsync();

            var dailyRevenue = paidOrdersToday.Sum(o => o.TotalAmount);
            var dailyOrdersCount = paidOrdersToday.Count;

            var activeTablesCount = await _context.Tables
                .CountAsync(t => t.Status == TableStatus.Occupied || t.Status == TableStatus.Reserved);

            var lowStockAlerts = await _context.InventoryItems
                .Where(i => i.QuantityInStock <= i.ReorderLevel)
                .ToListAsync();

            var popularItems = await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order!.OrderTime >= todayUtc && oi.Order.Status == OrderStatus.Paid)
                .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem!.Name })
                .Select(g => new PopularItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    ItemName = g.Key.Name,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(pi => pi.QuantitySold)
                .Take(5)
                .ToListAsync();

            return new DashboardSummaryDto
            {
                DailyRevenue = dailyRevenue,
                DailyOrdersCount = dailyOrdersCount,
                ActiveTablesCount = activeTablesCount,
                LowStockItemsCount = lowStockAlerts.Count,
                PopularItems = popularItems,
                LowStockAlerts = lowStockAlerts
            };
        }
    }
}
