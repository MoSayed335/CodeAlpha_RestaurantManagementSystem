using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Data;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public MenuController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            var menuItems = await _context.MenuItems
                .Include(m => m.Ingredients)
                    .ThenInclude(i => i.InventoryItem)
                .ToListAsync();
            return Ok(menuItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Ingredients)
                    .ThenInclude(i => i.InventoryItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null) return NotFound();
            return Ok(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItem menuItem)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (menuItem.Ingredients != null)
            {
                foreach (var ingredient in menuItem.Ingredients)
                {
                    ingredient.MenuItem = menuItem;
                    if (ingredient.InventoryItem != null)
                    {
                        _context.Entry(ingredient.InventoryItem).State = EntityState.Unchanged;
                    }
                }
            }

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItem updatedItem)
        {
            if (id != updatedItem.Id) return BadRequest("ID mismatch.");

            var dbItem = await _context.MenuItems
                .Include(m => m.Ingredients)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dbItem == null) return NotFound();

            dbItem.Name = updatedItem.Name;
            dbItem.Description = updatedItem.Description;
            dbItem.Price = updatedItem.Price;
            dbItem.Category = updatedItem.Category;
            dbItem.IsAvailable = updatedItem.IsAvailable;

            _context.MenuItemIngredients.RemoveRange(dbItem.Ingredients);
            
            if (updatedItem.Ingredients != null)
            {
                foreach (var ingredient in updatedItem.Ingredients)
                {
                    dbItem.Ingredients.Add(new MenuItemIngredient
                    {
                        MenuItemId = id,
                        InventoryItemId = ingredient.InventoryItemId,
                        QuantityRequired = ingredient.QuantityRequired
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(dbItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
