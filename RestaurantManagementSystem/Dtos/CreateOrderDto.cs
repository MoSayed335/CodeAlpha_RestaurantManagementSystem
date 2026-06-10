using System.Collections.Generic;

namespace RestaurantManagementSystem.Dtos
{
    public class CreateOrderDto
    {
        public int? TableId { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
