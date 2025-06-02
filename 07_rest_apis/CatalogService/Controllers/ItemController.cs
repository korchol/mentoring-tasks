using Microsoft.AspNetCore.Mvc;
using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CatalogService.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private readonly CatalogServiceDbContext _context;

        public ItemController(CatalogServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Items.AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(i => i.CategoryId == categoryId.Value);

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new
            {
                Data = items,
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] Item item)
        {
            if (_context.Items.Any(c => c.Id == item.Id))
            {
                return Conflict(new { error = $"A category with ID {item.Id} already exists." });
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item updatedItem)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = updatedItem.Name;
            item.Price = updatedItem.Price;
            item.CategoryId = updatedItem.CategoryId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}