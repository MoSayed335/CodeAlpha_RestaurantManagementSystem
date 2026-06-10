using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly RestaurantDbContext _context;

        public OrderService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // 1. If table is assigned, check and update Table status
            if (order.TableId.HasValue)
            {
                var table = await _context.Tables.FindAsync(order.TableId.Value);
                if (table == null)
                {
                    throw new ArgumentException("Table does not exist.");
                }
                table.Status = TableStatus.Occupied;
            }

            // 2. Load menu items and calculate total amount
            decimal totalAmount = 0;
            foreach (var item in order.OrderItems)
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Ingredients)
                        .ThenInclude(i => i.InventoryItem)
                    .FirstOrDefaultAsync(m => m.Id == item.MenuItemId);

                if (menuItem == null)
                {
                    throw new ArgumentException($"Menu item with ID {item.MenuItemId} not found.");
                }
                if (!menuItem.IsAvailable)
                {
                    throw new InvalidOperationException($"Menu item '{menuItem.Name}' is currently unavailable.");
                }

                item.UnitPrice = menuItem.Price;
                totalAmount += menuItem.Price * item.Quantity;
            }
            order.TotalAmount = totalAmount;
            order.OrderTime = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;

            // 3. Verify inventory has enough ingredients
            bool inventorySufficient = await VerifyInventoryForOrderAsync(order);
            if (!inventorySufficient)
            {
                throw new InvalidOperationException("Insufficient inventory to fulfill this order.");
            }

            // 4. Deduct inventory items
            await DeductInventoryAsync(order);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                        .ThenInclude(m => m.Ingredients)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            var oldStatus = order.Status;
            if (oldStatus == newStatus) return order;

            order.Status = newStatus;

            // Handlers for status transitions
            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                // Restore inventory ingredients if cancelled
                await RestoreInventoryAsync(order);

                // Free the table
                if (order.TableId.HasValue)
                {
                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                    if (table != null)
                    {
                        table.Status = TableStatus.Available;
                    }
                }
            }
            else if (newStatus == OrderStatus.Paid || newStatus == OrderStatus.Cancelled)
            {
                // Free the table when paid or cancelled
                if (order.TableId.HasValue)
                {
                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                    if (table != null)
                    {
                        // Check if there are other active orders on this table. If not, make available.
                        var activeOrdersCount = await _context.Orders
                            .AnyAsync(o => o.TableId == order.TableId && o.Id != order.Id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);
                        if (!activeOrdersCount)
                        {
                            table.Status = TableStatus.Available;
                        }
                    }
                }
            }
            else if (newStatus == OrderStatus.Preparing || newStatus == OrderStatus.Served)
            {
                // Ensure table is set to occupied
                if (order.TableId.HasValue)
                {
                    var table = await _context.Tables.FindAsync(order.TableId.Value);
                    if (table != null)
                    {
                        table.Status = TableStatus.Occupied;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> VerifyInventoryForOrderAsync(Order order)
        {
            // Gather all ingredients required for this order and their total sum
            var requiredIngredients = new Dictionary<int, double>();

            foreach (var orderItem in order.OrderItems)
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Ingredients)
                    .FirstOrDefaultAsync(m => m.Id == orderItem.MenuItemId);

                if (menuItem == null) continue;

                foreach (var ingredient in menuItem.Ingredients)
                {
                    if (requiredIngredients.ContainsKey(ingredient.InventoryItemId))
                    {
                        requiredIngredients[ingredient.InventoryItemId] += ingredient.QuantityRequired * orderItem.Quantity;
                    }
                    else
                    {
                        requiredIngredients[ingredient.InventoryItemId] = ingredient.QuantityRequired * orderItem.Quantity;
                    }
                }
            }

            // Check each ingredient against stock levels
            foreach (var kvp in requiredIngredients)
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(kvp.Key);
                if (inventoryItem == null || inventoryItem.QuantityInStock < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task DeductInventoryAsync(Order order)
        {
            foreach (var orderItem in order.OrderItems)
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Ingredients)
                    .FirstOrDefaultAsync(m => m.Id == orderItem.MenuItemId);

                if (menuItem == null) continue;

                foreach (var ingredient in menuItem.Ingredients)
                {
                    var inventoryItem = await _context.InventoryItems.FindAsync(ingredient.InventoryItemId);
                    if (inventoryItem != null)
                    {
                        inventoryItem.QuantityInStock -= ingredient.QuantityRequired * orderItem.Quantity;
                        if (inventoryItem.QuantityInStock < 0) inventoryItem.QuantityInStock = 0; // Safeguard
                    }
                }
            }
        }

        private async Task RestoreInventoryAsync(Order order)
        {
            foreach (var orderItem in order.OrderItems)
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Ingredients)
                    .FirstOrDefaultAsync(m => m.Id == orderItem.MenuItemId);

                if (menuItem == null) continue;

                foreach (var ingredient in menuItem.Ingredients)
                {
                    var inventoryItem = await _context.InventoryItems.FindAsync(ingredient.InventoryItemId);
                    if (inventoryItem != null)
                    {
                        inventoryItem.QuantityInStock += ingredient.QuantityRequired * orderItem.Quantity;
                    }
                }
            }
        }
    }
}
