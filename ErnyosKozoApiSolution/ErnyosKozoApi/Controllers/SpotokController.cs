using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotokController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpotokController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.Spotok.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var spot = await _context.Spotok.FindAsync(id);
            if (spot == null) return NotFound();
            return Ok(spot);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Spot spot)
        {
            _context.Spotok.Add(spot);
            await _context.SaveChangesAsync();
            return Ok(spot);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Spot spot)
        {
            if (id != spot.SpotID) return BadRequest();
            _context.Entry(spot).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var spot = await _context.Spotok.FindAsync(id);
            if (spot == null) return NotFound();

            _context.Spotok.Remove(spot);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
