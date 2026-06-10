using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public InventoryController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetInventory()
        {
            var inventory = await _context.InventoryItems.ToListAsync();
            return Ok(inventory);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInventoryItem([FromBody] InventoryItem item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInventoryItem), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] InventoryItem item)
        {
            if (id != item.Id) return BadRequest("ID mismatch.");

            var dbItem = await _context.InventoryItems.FindAsync(id);
            if (dbItem == null) return NotFound();

            dbItem.Name = item.Name;
            dbItem.QuantityInStock = item.QuantityInStock;
            dbItem.Unit = item.Unit;
            dbItem.ReorderLevel = item.ReorderLevel;

            await _context.SaveChangesAsync();
            return Ok(dbItem);
        }

        [HttpPut("{id}/restock")]
        public async Task<IActionResult> RestockItem(int id, [FromBody] double quantity)
        {
            if (quantity <= 0) return BadRequest("Quantity must be greater than zero.");

            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return NotFound();

            item.QuantityInStock += quantity;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return NotFound();

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
