using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Services
{
    public class ReservationService : IReservationService
    {
        private readonly RestaurantDbContext _context;

        public ReservationService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Table)
                .OrderByDescending(r => r.ReservationTime)
                .ToListAsync();
        }

        public async Task<Reservation?> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Reservation> CreateReservationAsync(Reservation reservation)
        {
            // Validate table exists and is available
            var table = await _context.Tables.FindAsync(reservation.TableId);
            if (table == null)
            {
                throw new ArgumentException("Table does not exist.");
            }

            if (reservation.NumberOfGuests > table.Capacity)
            {
                throw new ArgumentException($"Number of guests ({reservation.NumberOfGuests}) exceeds table capacity ({table.Capacity}).");
            }

            var isAvailable = await CheckTableAvailabilityAsync(reservation.TableId, reservation.ReservationTime, reservation.NumberOfGuests);
            if (!isAvailable)
            {
                throw new InvalidOperationException("Table is already reserved or occupied at this time.");
            }

            reservation.Status = ReservationStatus.Pending;
            _context.Reservations.Add(reservation);
            
            // If reservation is within 1 hour of now, set table status to Reserved.
            if (Math.Abs((reservation.ReservationTime - DateTime.UtcNow).TotalHours) <= 1)
            {
                table.Status = TableStatus.Reserved;
            }

            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<bool> CancelReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            reservation.Status = ReservationStatus.Cancelled;
            
            // Release table if it was reserved
            var table = await _context.Tables.FindAsync(reservation.TableId);
            if (table != null && table.Status == TableStatus.Reserved)
            {
                table.Status = TableStatus.Available;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            reservation.Status = ReservationStatus.Confirmed;
            
            // Set table status to reserved
            var table = await _context.Tables.FindAsync(reservation.TableId);
            if (table != null)
            {
                table.Status = TableStatus.Reserved;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            reservation.Status = ReservationStatus.Completed;

            var table = await _context.Tables.FindAsync(reservation.TableId);
            if (table != null && table.Status == TableStatus.Reserved)
            {
                table.Status = TableStatus.Available;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckTableAvailabilityAsync(int tableId, DateTime reservationTime, int numberOfGuests)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null) return false;
            if (numberOfGuests > table.Capacity) return false;

            // Check if table is occupied right now (if reservation time is close to now)
            if (Math.Abs((reservationTime - DateTime.UtcNow).TotalMinutes) < 120) // 2-hour window
            {
                if (table.Status == TableStatus.Occupied)
                {
                    return false;
                }
            }

            // Check conflicting reservations (within a 2-hour window of the requested reservation time)
            var startWindow = reservationTime.AddHours(-2);
            var endWindow = reservationTime.AddHours(2);

            var conflict = await _context.Reservations
                .AnyAsync(r => r.TableId == tableId &&
                               r.Status != ReservationStatus.Cancelled &&
                               r.Status != ReservationStatus.Completed &&
                               r.ReservationTime > startWindow &&
                               r.ReservationTime < endWindow);

            return !conflict;
        }
    }
}
