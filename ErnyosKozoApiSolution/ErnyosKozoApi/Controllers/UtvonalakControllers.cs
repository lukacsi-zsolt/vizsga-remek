using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtvonalakController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtvonalakController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.Utvonalak.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var utvonal = await _context.Utvonalak.FindAsync(id);
            if (utvonal == null) return NotFound();
            return Ok(utvonal);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Utvonal utvonal)
        {
            _context.Utvonalak.Add(utvonal);
            await _context.SaveChangesAsync();
            return Ok(utvonal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Utvonal utvonal)
        {
            if (id != utvonal.UtvonalID) return BadRequest();
            _context.Entry(utvonal).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var utvonal = await _context.Utvonalak.FindAsync(id);
            if (utvonal == null) return NotFound();

            _context.Utvonalak.Remove(utvonal);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
