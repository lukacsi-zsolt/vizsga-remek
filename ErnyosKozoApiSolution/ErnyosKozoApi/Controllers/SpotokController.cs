using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
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
        {
            var spotok = await _context.Spotok
                .OrderBy(s => s.Nev)
                .ToListAsync();

            return Ok(spotok);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var spot = await _context.Spotok.FindAsync(id);
            if (spot == null) return NotFound();
            return Ok(spot);
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var spot = await _context.Spotok
                .FirstOrDefaultAsync(s => s.Slug == slug);

            if (spot == null) return NotFound();

            return Ok(spot);
        }

        [Authorize]
        [HttpPost("javaslat")]
        public async Task<IActionResult> SuggestSpot(SpotJavaslat dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Nev))
                return BadRequest("A spot neve kötelező.");

            if (dto.Lat == null || dto.Lon == null)
                return BadRequest("A koordináták kötelezőek.");

            dto.SpotJavaslatID = 0;
            dto.BekuldoFelhasznaloID = userId;
            dto.Letrehozva = DateTime.UtcNow;
            dto.Feldolgozva = false;

            _context.SpotJavaslatok.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Spot spot)
        {
            if (!User.IsAdmin())
                return Forbid();

            if (string.IsNullOrWhiteSpace(spot.Nev))
                return BadRequest("A spot neve kötelező.");

            if (spot.Lat == null || spot.Lon == null)
                return BadRequest("A koordináták kötelezőek.");

            if (string.IsNullOrWhiteSpace(spot.Slug))
                spot.Slug = Slugify(spot.Nev);

            var slugExists = await _context.Spotok.AnyAsync(s => s.Slug == spot.Slug);
            if (slugExists)
                spot.Slug = $"{spot.Slug}-{Guid.NewGuid().ToString("N")[..6]}";

            _context.Spotok.Add(spot);
            await _context.SaveChangesAsync();

            return Ok(spot);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Spot spot)
        {
            if (!User.IsAdmin())
                return Forbid();

            if (id != spot.SpotID) return BadRequest();

            _context.Entry(spot).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsAdmin())
                return Forbid();

            var spot = await _context.Spotok.FindAsync(id);
            if (spot == null) return NotFound();

            _context.Spotok.Remove(spot);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static string Slugify(string text)
        {
            return text
                .Trim()
                .ToLower()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ö", "o")
                .Replace("ő", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ű", "u")
                .Replace(" ", "-");
        }
    }
}