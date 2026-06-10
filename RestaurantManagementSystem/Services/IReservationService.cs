using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<Reservation>> GetAllReservationsAsync();
        Task<Reservation?> GetReservationByIdAsync(int id);
        Task<Reservation> CreateReservationAsync(Reservation reservation);
        Task<bool> CancelReservationAsync(int id);
        Task<bool> ConfirmReservationAsync(int id);
        Task<bool> CompleteReservationAsync(int id);
        Task<bool> CheckTableAvailabilityAsync(int tableId, DateTime reservationTime, int numberOfGuests);
    }
}
