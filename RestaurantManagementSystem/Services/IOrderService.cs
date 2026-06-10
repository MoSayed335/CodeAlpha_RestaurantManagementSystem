using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> UpdateOrderStatusAsync(int id, OrderStatus newStatus);
        Task<bool> VerifyInventoryForOrderAsync(Order order);
    }
}
