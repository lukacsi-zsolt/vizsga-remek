using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ErnyosKozoApi.Controllers
{
    [Authorize]                 // Alapértelmezetten minden végpont bejelentkezést igényel
    [ApiController]             // API viselkedés
    [Route("api/[controller]")] // Útvonal: api/Kovetesek
    public class KovetesekController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KovetesekController(AppDbContext context)
        {
            _context = context;
        }

        // ===== KÖVETÉS / KIKÖVETÉS (TOGGLE) =====
        // POST api/Kovetesek/{kovetettFelhasznaloId}
        // Ez egy "toggle" (váltó) végpont: ha még nem követi → bekövetés, ha már követi → kikövetés
        // Egyetlen végpont két műveletre – ez egyszerűsíti a frontend logikát
        [HttpPost("{kovetettFelhasznaloId}")]
        public async Task<IActionResult> KovetesValtas(int kovetettFelhasznaloId)
        {
            // Bejelentkezett felhasználó ID-jának kiolvasása a JWT tokenből
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            // Önmagát nem követheti senki
            if (userId == kovetettFelhasznaloId)
                return BadRequest("Saját magadat nem követheted.");

            // Megnézzük, létezik-e már követés a két felhasználó között
            var letezo = await _context.Kovetesek.FirstOrDefaultAsync(k =>
                k.KovetoFelhasznaloID == userId &&
                k.KovetettFelhasznaloID == kovetettFelhasznaloId);

            // Ha már követi → KIKÖVETÉS (törlés)
            if (letezo != null)
            {
                _context.Kovetesek.Remove(letezo);
                await _context.SaveChangesAsync();
                return Ok(new { koveti = false });  // Jelezzük a frontendnek: már nem követi
            }

            // Ha még NEM követi → BEKÖVETÉS (új rekord létrehozása)
            var uj = new Kovetes
            {
                KovetoFelhasznaloID = userId,                   // Ki követ (a bejelentkezett felhasználó)
                KovetettFelhasznaloID = kovetettFelhasznaloId,  // Kit követ (az URL-ből kapott ID)
                Letrehozva = DateTime.UtcNow                    // Követés időpontja
            };

            _context.Kovetesek.Add(uj);
            await _context.SaveChangesAsync();

            return Ok(new { koveti = true });   // Jelezzük a frontendnek: most már követi
        }

        // ===== KÖVETÉS ÁLLAPOT LEKÉRDEZÉSE =====
        // GET api/Kovetesek/allapot/{felhasznaloId}
        // Megmondja, hogy a bejelentkezett felhasználó követi-e az adott felhasználót
        // A frontend ezzel dönti el, hogy "Követés" vagy "Kikövetés" gombot mutasson
        [AllowAnonymous]    // Token nélkül is elérhető (vendég felhasználóknak is kell az állapot)
        [HttpGet("allapot/{felhasznaloId}")]
        public async Task<IActionResult> GetKovetesAllapot(int felhasznaloId)
        {
            // Ha nincs bejelentkezett felhasználó → egyszerűen "nem követi"
            // Nem Unauthorized-ot küldünk, mert ez egy publikus végpont –
            // vendég felhasználónak is kell tudnia, hogy az alapállapot "nem követi"
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Ok(new { koveti = false });

            // AnyAsync: van-e legalább egy ilyen rekord? (true/false – hatékonyabb mint Count > 0)
            var koveti = await _context.Kovetesek.AnyAsync(k =>
                k.KovetoFelhasznaloID == userId &&
                k.KovetettFelhasznaloID == felhasznaloId);

            // Anonim objektum: { "koveti": true } vagy { "koveti": false }
            return Ok(new { koveti });
        }
    }
}