namespace RestaurantManagementSystem.Models
{
    public enum TableStatus
    {
        Available,
        Occupied,
        Reserved
    }

    public class Table
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public TableStatus Status { get; set; } = TableStatus.Available;
    }
}
