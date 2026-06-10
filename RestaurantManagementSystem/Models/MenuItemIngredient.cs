using System.Text.Json.Serialization;

namespace RestaurantManagementSystem.Models
{
    public class MenuItemIngredient
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        
        [JsonIgnore]
        public MenuItem? MenuItem { get; set; }
        
        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        
        public double QuantityRequired { get; set; }
    }
}
