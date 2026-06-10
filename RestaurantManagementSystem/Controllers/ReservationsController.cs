using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Services;

namespace RestaurantManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return Ok(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            try
            {
                var created = await _reservationService.CreateReservationAsync(reservation);
                return CreatedAtAction(nameof(GetReservation), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            var result = await _reservationService.ConfirmReservationAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Reservation confirmed." });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var result = await _reservationService.CancelReservationAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Reservation cancelled." });
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteReservation(int id)
        {
            var result = await _reservationService.CompleteReservationAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Reservation completed." });
        }

        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromQuery] int tableId, [FromQuery] DateTime time, [FromQuery] int guests)
        {
            var available = await _reservationService.CheckTableAvailabilityAsync(tableId, time, guests);
            return Ok(new { available });
        }
    }
}
