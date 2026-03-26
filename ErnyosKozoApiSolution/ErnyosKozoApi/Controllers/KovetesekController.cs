using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ErnyosKozoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KovetesekController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KovetesekController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("{kovetettFelhasznaloId}")]
        public async Task<IActionResult> KovetesValtas(int kovetettFelhasznaloId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            if (userId == kovetettFelhasznaloId)
                return BadRequest("Saját magadat nem követheted.");

            var letezo = await _context.Kovetesek.FirstOrDefaultAsync(k =>
                k.KovetoFelhasznaloID == userId &&
                k.KovetettFelhasznaloID == kovetettFelhasznaloId);

            if (letezo != null)
            {
                _context.Kovetesek.Remove(letezo);
                await _context.SaveChangesAsync();
                return Ok(new { koveti = false });
            }

            var uj = new Kovetes
            {
                KovetoFelhasznaloID = userId,
                KovetettFelhasznaloID = kovetettFelhasznaloId,
                Letrehozva = DateTime.UtcNow
            };

            _context.Kovetesek.Add(uj);
            await _context.SaveChangesAsync();

            return Ok(new { koveti = true });
        }

        [AllowAnonymous]
        [HttpGet("allapot/{felhasznaloId}")]
        public async Task<IActionResult> GetKovetesAllapot(int felhasznaloId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Ok(new { koveti = false });

            var koveti = await _context.Kovetesek.AnyAsync(k =>
                k.KovetoFelhasznaloID == userId &&
                k.KovetettFelhasznaloID == felhasznaloId);

            return Ok(new { koveti });
        }
    }
}