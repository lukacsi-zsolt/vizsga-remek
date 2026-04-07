using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]             // API viselkedés
    [Route("api/[controller]")] // Útvonal: api/Spotok
    // NINCS osztályszintű [Authorize] → olvasási végpontok publikusak
    public class SpotokController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpotokController(AppDbContext context)
        {
            _context = context;
        }

        // ===== ÖSSZES SPOT LISTÁZÁSA =====
        // GET api/Spotok
        // Publikus – az összes repülős helyszín (spot) név szerinti sorrendben
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var spotok = await _context.Spotok
                .OrderBy(s => s.Nev)    // ABC sorrendben
                .ToListAsync();

            return Ok(spotok);  // Teljes entitás visszaküldése (nincs DTO)
        }

        // ===== EGY SPOT LEKÉRDEZÉSE ID ALAPJÁN =====
        // GET api/Spotok/{id}
        // Az "{id:int}" route constraint biztosítja, hogy csak egész szám illeszkedjen
        // Ez megkülönbözteti a slug alapú kéréstől (ami string)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var spot = await _context.Spotok.FindAsync(id); // PK alapján keresés
            if (spot == null) return NotFound();
            return Ok(spot);
        }

        // ===== EGY SPOT LEKÉRDEZÉSE SLUG ALAPJÁN =====
        // GET api/Spotok/slug/{slug}
        // A slug az URL-barát azonosító (pl. "bakony-hegy")
        // A frontend ezt használja szép URL-ek építéséhez: /spotok/bakony-hegy
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var spot = await _context.Spotok
                .FirstOrDefaultAsync(s => s.Slug == slug);  // Slug alapján keresés

            if (spot == null) return NotFound();

            return Ok(spot);
        }

        // ===== SPOT JAVASLAT BEKÜLDÉSE =====
        // POST api/Spotok/javaslat
        // Bejelentkezett felhasználó új helyszínt javasolhat – az admin majd elfogadja vagy elutasítja
        [Authorize]
        [HttpPost("javaslat")]
        public async Task<IActionResult> SuggestSpot(SpotJavaslat dto)
        {
            // Felhasználó ID kiolvasása – User.GetUserId() a Helpers kiterjesztő metódusa
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            // ===== SZERVER OLDALI VALIDÁCIÓ =====
            // Fontos: a kliensnek sosem bízunk meg teljesen, mindig a szerveren is ellenőrzünk
            if (string.IsNullOrWhiteSpace(dto.Nev))
                return BadRequest("A spot neve kötelező.");

            if (dto.Lat == null || dto.Lon == null)
                return BadRequest("A koordináták kötelezőek.");

            // ===== BIZTONSÁGI MEZŐK FELÜLÍRÁSA =====
            // A klienstől érkező értékeket felülírjuk szerver oldali értékekkel
            // Ez megakadályozza, hogy a kliens manipulálja ezeket a mezőket
            dto.SpotJavaslatID = 0;             // 0 → az adatbázis generálja az ID-t (IDENTITY)
            dto.BekuldoFelhasznaloID = userId;  // Mindig a tokenből jövő ID, nem a klienstől
            dto.Letrehozva = DateTime.UtcNow;   // Szerver oldali időbélyeg
            dto.Feldolgozva = false;            // Új javaslat → még nincs feldolgozva

            _context.SpotJavaslatok.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        // ===== ÚJ SPOT LÉTREHOZÁSA (ADMIN) =====
        // POST api/Spotok
        // Csak admin hozhat létre közvetlenül spotot (a javaslat elfogadáson kívül)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Spot spot)
        {
            if (!User.IsAdmin())
                return Forbid();

            // Validáció
            if (string.IsNullOrWhiteSpace(spot.Nev))
                return BadRequest("A spot neve kötelező.");

            if (spot.Lat == null || spot.Lon == null)
                return BadRequest("A koordináták kötelezőek.");

            // Ha nincs slug megadva → automatikusan generáljuk a névből
            if (string.IsNullOrWhiteSpace(spot.Slug))
                spot.Slug = Slugify(spot.Nev);

            // Slug egyediségének biztosítása – ha már létezik, GUID-ot fűzünk hozzá
            var slugExists = await _context.Spotok.AnyAsync(s => s.Slug == spot.Slug);
            if (slugExists)
                spot.Slug = $"{spot.Slug}-{Guid.NewGuid().ToString("N")[..6]}";

            _context.Spotok.Add(spot);
            await _context.SaveChangesAsync();

            return Ok(spot);
        }

        // ===== SPOT MÓDOSÍTÁSA (ADMIN) =====
        // PUT api/Spotok/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Spot spot)
        {
            if (!User.IsAdmin())
                return Forbid();

            // ID egyezés ellenőrzése (URL vs. body)
            if (id != spot.SpotID) return BadRequest();

            // EntityState.Modified: az EF Core-nak jelezzük, hogy az entitás összes mezője módosult
            // Ez az "untracked" frissítési minta – az entitás nem az adatbázisból jött (FindAsync-kal),
            // hanem a klienstől, ezért manuálisan kell beállítani az állapotát
            _context.Entry(spot).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ===== SPOT TÖRLÉSE (ADMIN) =====
        // DELETE api/Spotok/{id}
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

        // ===== SLUG GENERÁLÓ SEGÉDMETÓDUS =====
        // private static: csak ezen az osztályon belül elérhető, és nem függ példány adatoktól
        // Ugyanaz a logika, mint az AdminController-ben – a magyar ékezetes karakterek cseréje
        private static string Slugify(string text)
        {
            return text
                .Trim()             // Szóközök levágása
                .ToLower()          // Kisbetűsítés
                // Magyar ékezetes karakterek → ékezet nélküli megfelelőjük
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ö", "o")
                .Replace("ő", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ű", "u")
                .Replace(" ", "-"); // Szóközök → kötőjelek
        }
    }
}