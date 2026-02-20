using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class FelhasznalokController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FelhasznalokController(AppDbContext context)
        {
            _context = context;
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetFelhasznalok()
        {
            return Ok(await _context.Felhasznalok.ToListAsync());
        }

        // READ ONE
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFelhasznalo(int id)
        {
            var felhasznalo = await _context.Felhasznalok.FindAsync(id);
            if (felhasznalo == null) return NotFound();
            return Ok(felhasznalo);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> CreateFelhasznalo(Felhasznalo felhasznalo)
        {
            _context.Felhasznalok.Add(felhasznalo);
            await _context.SaveChangesAsync();
            return Ok(felhasznalo);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFelhasznalo(int id, Felhasznalo felhasznalo)
        {
            if (id != felhasznalo.FelhasznaloID) return BadRequest();

            _context.Entry(felhasznalo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFelhasznalo(int id)
        {
            var felhasznalo = await _context.Felhasznalok.FindAsync(id);
            if (felhasznalo == null) return NotFound();

            _context.Felhasznalok.Remove(felhasznalo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
