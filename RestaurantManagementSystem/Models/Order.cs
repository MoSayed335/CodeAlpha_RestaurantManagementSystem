using System;
using System.Collections.Generic;

namespace RestaurantManagementSystem.Models
{
    public enum OrderStatus
    {
        Pending,
        Preparing,
        Served,
        Paid,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public int? TableId { get; set; }
        public Table? Table { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
