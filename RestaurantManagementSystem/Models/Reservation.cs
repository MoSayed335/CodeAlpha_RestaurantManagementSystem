using System;

namespace RestaurantManagementSystem.Models
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table? Table { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime ReservationTime { get; set; }
        public int NumberOfGuests { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    }
}
