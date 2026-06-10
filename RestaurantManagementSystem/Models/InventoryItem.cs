namespace RestaurantManagementSystem.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double QuantityInStock { get; set; }
        public string Unit { get; set; } = string.Empty; // e.g. "g", "ml", "pcs"
        public double ReorderLevel { get; set; } // Quantity at or below which to alert
    }
}
