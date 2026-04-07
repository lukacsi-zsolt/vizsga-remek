using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SzarnysegedShared.DTOs.ForumDTOs;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]                 // API viselkedés: automatikus model validáció
    [Route("api/[controller]")]     // Útvonal: api/Forum

    // MEGJEGYZÉS: itt NINCS osztályszintű [Authorize]!
    // Ez azt jelenti, hogy alapértelmezetten a végpontok publikusak,
    // és csak azoknál van [Authorize], ahol bejelentkezés kell (pl. létrehozás, törlés)
    public class ForumController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ForumController(AppDbContext context)
        {
            _context = context;
        }

        // ===== SEGÉDMETÓDUS: RELATÍV URL → ABSZOLÚT URL =====
        // Ugyanaz a logika, mint a FelhasznalokController-ben, de kiszervezve külön metódusba
        // Ez a DRY elv alkalmazása (Don't Repeat Yourself – ne ismételd magad)
        private string? ToAbsoluteUrl(string? relativeOrAbsoluteUrl)
        {
            // Ha null vagy üres → null
            if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl))
                return null;

            // Ha már abszolút URL (pl. "https://...") → meghagyjuk
            if (Uri.TryCreate(relativeOrAbsoluteUrl, UriKind.Absolute, out _))
                return relativeOrAbsoluteUrl;

            // Ha relatív (pl. "/uploads/forum/kep.jpg") → teljes URL-t építünk
            return $"{Request.Scheme}://{Request.Host}{relativeOrAbsoluteUrl}";
        }

        // ===== ÖSSZES BEJEGYZÉS LISTÁZÁSA =====
        // GET api/Forum/bejegyzesek
        // Publikus végpont – nincs [Authorize], bárki megtekintheti a fórumot
        [HttpGet("bejegyzesek")]
        public async Task<ActionResult<List<BejegyzesListaDto>>> GetBejegyzesek()
        {
            // Bejegyzések lekérdezése a kapcsolódó entitásokkal
            var lista = await _context.Bejegyzesek
                .Include(b => b.Felhasznalo)            // Szerző adatai (JOIN)
                .Include(b => b.Spot)                   // Kapcsolódó spot (helyszín)
                .Include(b => b.Kommentek)              // Kommentek (a Count-hoz kell)
                .OrderByDescending(b => b.Letrehozva)   // Legújabb elöl
                .ToListAsync();

            // Memóriában mappelés DTO-ba – a ToAbsoluteUrl() miatt nem lehet EF LINQ-ban
            var result = lista.Select(b => new BejegyzesListaDto
            {
                BejegyzesID = b.BejegyzesID,
                Cim = b.Cim,
                Tartalom = b.Tartalom,
                KepUrl = ToAbsoluteUrl(b.KepUrl),                    // Kép URL → abszolúttá alakítva
                Letrehozva = b.Letrehozva,
                FelhasznaloID = b.FelhasznaloID,
                FelhasznaloNev = b.Felhasznalo?.FelhasznaloNev,
                TeljesNev = b.Felhasznalo?.TeljesNev,
                AvatarUrl = ToAbsoluteUrl(b.Felhasznalo?.AvatarUrl), // Avatar URL → abszolúttá alakítva
                SpotID = b.SpotID,
                SpotNev = b.Spot?.Nev,
                SpotSlug = b.Spot?.Slug,
                KommentekSzama = b.Kommentek.Count
            }).ToList();

            return Ok(result);
        }

        // ===== EGY BEJEGYZÉS LEKÉRDEZÉSE =====
        // GET api/Forum/bejegyzesek/{id}
        // Publikus – egy konkrét bejegyzés részletei (pl. bejegyzés oldal megnyitásakor)
        [HttpGet("bejegyzesek/{id}")]
        public async Task<ActionResult<BejegyzesListaDto>> GetBejegyzes(int id)
        {
            var x = await _context.Bejegyzesek
                .Include(b => b.Felhasznalo)
                .Include(b => b.Spot)
                .Include(b => b.Kommentek)
                .FirstOrDefaultAsync(b => b.BejegyzesID == id); // ID alapján keresés

            if (x == null)
                return NotFound();  // 404 ha nem létezik

            // Egyetlen entitás mappelése DTO-ba (ugyanaz a logika, mint a listánál)
            var result = new BejegyzesListaDto
            {
                BejegyzesID = x.BejegyzesID,
                Cim = x.Cim,
                Tartalom = x.Tartalom,
                KepUrl = ToAbsoluteUrl(x.KepUrl),
                Letrehozva = x.Letrehozva,
                FelhasznaloID = x.FelhasznaloID,
                FelhasznaloNev = x.Felhasznalo?.FelhasznaloNev,
                TeljesNev = x.Felhasznalo?.TeljesNev,
                AvatarUrl = ToAbsoluteUrl(x.Felhasznalo?.AvatarUrl),
                SpotID = x.SpotID,
                SpotNev = x.Spot?.Nev,
                SpotSlug = x.Spot?.Slug,
                KommentekSzama = x.Kommentek.Count
            };

            return Ok(result);
        }

        // ===== ÚJ BEJEGYZÉS LÉTREHOZÁSA =====
        // POST api/Forum/bejegyzesek
        // [Authorize] – csak bejelentkezett felhasználó hozhat létre bejegyzést
        [Authorize]
        [HttpPost("bejegyzesek")]
        public async Task<IActionResult> CreateBejegyzes(BejegyzesLetrehozasDto dto)
        {
            // Bejelentkezett felhasználó ID-jának kiolvasása a JWT tokenből
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            // Új bejegyzés entitás létrehozása
            var bejegyzes = new Bejegyzes
            {
                FelhasznaloID = userId,         // A szerző mindig a bejelentkezett felhasználó (nem a klienstől jön!)
                Cim = dto.Cim,
                Tartalom = dto.Tartalom,
                KepUrl = string.IsNullOrWhiteSpace(dto.KepUrl) ? null : dto.KepUrl, // Üres string → null
                SpotID = dto.SpotID,            // Opcionális: melyik spothoz tartozik a bejegyzés
                Letrehozva = DateTime.UtcNow    // Szerver oldali időbélyeg (nem bízunk a kliensben)
            };

            _context.Bejegyzesek.Add(bejegyzes);
            await _context.SaveChangesAsync();

            // Visszaadjuk az új bejegyzés ID-ját (a frontend ezzel tud navigálni az új bejegyzésre)
            return Ok(bejegyzes.BejegyzesID);
        }

        // ===== KOMMENTEK LEKÉRDEZÉSE (FA STRUKTÚRÁBAN) =====
        // GET api/Forum/bejegyzesek/{id}/kommentek
        // Publikus – egy bejegyzéshez tartozó kommentek HIERARCHIKUS (szülő-gyerek) struktúrában
        [HttpGet("bejegyzesek/{id}/kommentek")]
        public async Task<ActionResult<List<KommentDto>>> GetKommentek(int id)
        {
            // Az összes komment lekérdezése ehhez a bejegyzéshez (lapos lista)
            var kommentek = await _context.Kommentek
                .Include(k => k.Felhasznalo)        // Komment szerzőjének adatai
                .Where(k => k.BejegyzesID == id)    // Csak ehhez a bejegyzéshez tartozók
                .OrderBy(k => k.Letrehozva)         // Időrendi sorrend
                .ToListAsync();


            // ===== REKURZÍV FA ÉPÍTÉS =====
            // A kommentek fa (tree) struktúrába rendezése – a válaszok a szülő komment alá kerülnek
            // Ez egy lokális (belső) függvény – csak ezen a metóduson belül létezik
            List<KommentDto> BuildTree(int? szuloId)
            {
                return kommentek
                    // Szűrés: azok a kommentek, amelyeknek a szülője az adott szuloId
                    // Ha szuloId == null → a gyökér szintű (legfelső) kommentek
                    .Where(k => k.SzuloKommentID == szuloId)
                    .Select(k => new KommentDto
                    {
                        KommentID = k.KommentID,
                        BejegyzesID = k.BejegyzesID,
                        SzuloKommentID = k.SzuloKommentID,
                        FelhasznaloID = k.FelhasznaloID,
                        FelhasznaloNev = k.Felhasznalo?.FelhasznaloNev,
                        TeljesNev = k.Felhasznalo?.TeljesNev,
                        AvatarUrl = ToAbsoluteUrl(k.Felhasznalo?.AvatarUrl),
                        Tartalom = k.Tartalom,
                        Letrehozva = k.Letrehozva,
                        // REKURZIÓ: ezen komment gyerekeit is felépítjük (válaszok a válaszra)
                        Valaszok = BuildTree(k.KommentID)
                    })
                    .ToList();
            }

            // A fa építés indítása a gyökértől (szuloId = null → legfelső szintű kommentek)
            return Ok(BuildTree(null));
        }

        // ===== ÚJ KOMMENT LÉTREHOZÁSA =====
        // POST api/Forum/kommentek
        [Authorize]
        [HttpPost("kommentek")]
        public async Task<IActionResult> CreateKomment(KommentLetrehozasDto dto)
        {
            // Felhasználó azonosítása a tokenből
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var komment = new Komment
            {
                BejegyzesID = dto.BejegyzesID,          // Melyik bejegyzéshez tartozik
                SzuloKommentID = dto.SzuloKommentID,    // Melyik kommentre válaszol (null ha gyökér szintű)
                FelhasznaloID = userId,                 // A szerző (tokenből, nem klienstől!)
                Tartalom = dto.Tartalom,
                Letrehozva = DateTime.UtcNow            // Szerver oldali időbélyeg
            };

            _context.Kommentek.Add(komment);
            await _context.SaveChangesAsync();

            return Ok(komment.KommentID);  // Az új komment ID-ja
        }

        // ===== FÓRUM KÉP FELTÖLTÉS =====
        // POST api/Forum/upload-image
        // Bejegyzésekhez tartozó képek feltöltése – alaposabb validáció, mint az AuthController-ben
        [Authorize]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadForumImage(IFormFile file)
        {
            // Felhasználó azonosítása (csak bejelentkezett felhasználó tölthet fel)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out _))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("Nincs fájl feltöltve.");

            // ===== FÁJL VALIDÁCIÓ =====
            // Engedélyezett kiterjesztések – csak képfájlok
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();    // Kiterjesztés kisbetűsítve

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Csak .jpg, .jpeg, .png vagy .webp fájl tölthető fel.");

            // Méretkorlát: maximum 8 MB (8 * 1024 * 1024 byte)
            if (file.Length > 8 * 1024 * 1024)
                return BadRequest("A fájl túl nagy. Maximum 8 MB.");

            // Célmappa: wwwroot/uploads/forum
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),    // Az alkalmazás gyökérmappája
                "wwwroot",
                "uploads",
                "forum");

            // Mappa létrehozása ha nem létezik (CreateDirectory nem dob hibát ha már van)
            Directory.CreateDirectory(uploadsFolder);

            // Egyedi fájlnév GUID-dal (névütközés elkerülése + biztonsági szempont)
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Fájl mentése lemezre
            // "await using" – aszinkron dispose: a stream biztonságosan bezáródik
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Relatív URL-t adunk vissza – a teljes URL-t majd a lekérdezésnél építjük össze
            var imageUrl = $"/uploads/forum/{fileName}";
            return Ok(new { imageUrl });
        }

        // ===== BEJEGYZÉS TÖRLÉSE (ADMIN) =====
        // DELETE api/Forum/bejegyzesek/{id}
        // Csak admin törölhet bejegyzést
        [Authorize]
        [HttpDelete("bejegyzesek/{id}")]
        public async Task<IActionResult> DeleteBejegyzes(int id)
        {
            // Admin jogosultság ellenőrzése (a Helpers/ClaimsPrincipalExtensions-ből)
            if (!User.IsAdmin())
                return Forbid();    // 403 Forbidden – van jogosultsága belépni, de nincs joga törölni

            var bejegyzes = await _context.Bejegyzesek.FindAsync(id);
            if (bejegyzes == null)
                return NotFound();

            _context.Bejegyzesek.Remove(bejegyzes);
            await _context.SaveChangesAsync();

            return NoContent();     // 204 – sikeres törlés
        }

        // ===== KOMMENT TÖRLÉSE (ADMIN) =====
        // DELETE api/Forum/kommentek/{id}
        [Authorize]
        [HttpDelete("kommentek/{id}")]
        public async Task<IActionResult> DeleteKomment(int id)
        {
            if (!User.IsAdmin())
                return Forbid();

            var komment = await _context.Kommentek.FindAsync(id);
            if (komment == null)
                return NotFound();

            _context.Kommentek.Remove(komment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}