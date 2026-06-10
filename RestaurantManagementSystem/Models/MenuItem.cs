using System.Collections.Generic;

namespace RestaurantManagementSystem.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty; // e.g., Starter, Main, Dessert, Beverage
        public bool IsAvailable { get; set; } = true;
        
        public List<MenuItemIngredient> Ingredients { get; set; } = new();
    }
}
