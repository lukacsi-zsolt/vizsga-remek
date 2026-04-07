using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;
using SzarnysegedShared.DTOs.ForumDTOs;

namespace ErnyosKozoApi.Controllers
{
    [Authorize]                     // Alapértelmezetten minden végpont bejelentkezést igényel
    [ApiController]                 // API viselkedés: automatikus model validáció
    [Route("api/[controller]")]     // Útvonal: api/Felhasznalok
    public class FelhasznalokController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Konstruktor – DI-ból kapjuk az adatbázis kontextust
        public FelhasznalokController(AppDbContext context)
        {
            _context = context;
        }

        // ===== CRUD MŰVELETEK =====
        // A CRUD = Create, Read, Update, Delete – az alapvető adatkezelési műveletek

        // ===== READ ALL – Összes felhasználó lekérdezése =====
        // GET api/Felhasznalok
        [HttpGet]
        public async Task<IActionResult> GetFelhasznalok()
        {
            // Az összes felhasználó visszaadása listaként
            // FIGYELEM: ez az egész entitást visszaadja (jelszó hash-sel együtt!) – éles projektben DTO-t kellene használni
            return Ok(await _context.Felhasznalok.ToListAsync());
        }

        // ===== READ ONE – Egy felhasználó lekérdezése ID alapján =====
        // GET api/Felhasznalok/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFelhasznalo(int id)
        {
            // FindAsync: elsődleges kulcs (Primary Key) alapján keres – gyorsabb mint a Where
            var felhasznalo = await _context.Felhasznalok.FindAsync(id);
            if (felhasznalo == null) return NotFound(); // 404 ha nem létezik
            return Ok(felhasznalo);
        }

        // ===== CREATE – Új felhasználó létrehozása =====
        // POST api/Felhasznalok
        [HttpPost]
        public async Task<IActionResult> CreateFelhasznalo(Felhasznalo felhasznalo)
        {
            // Az entitás hozzáadása a kontextushoz (még csak memóriában, nincs mentve)
            _context.Felhasznalok.Add(felhasznalo);
            // Mentés az adatbázisba – itt hajtja végre az INSERT SQL-t
            await _context.SaveChangesAsync();
            return Ok(felhasznalo); // Visszaadjuk a létrehozott entitást (az ID-val együtt, amit az adatbázis generált)
        }

        // ===== UPDATE – Felhasználó módosítása =====
        // PUT api/Felhasznalok/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFelhasznalo(int id, Felhasznalo felhasznalo)
        {
            // Biztonsági ellenőrzés: az URL-ben lévő ID egyezik-e a body-ban küldött entitás ID-jával
            if (id != felhasznalo.FelhasznaloID) return BadRequest();

            // Az entitás állapotának beállítása "Modified"-ra
            // Ez jelzi az EF Core-nak, hogy az összes mezőt frissítse az adatbázisban (UPDATE SQL)
            _context.Entry(felhasznalo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent(); // 204 – sikeres módosítás
        }

        // ===== DELETE – Felhasználó törlése =====
        // DELETE api/Felhasznalok/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFelhasznalo(int id)
        {
            var felhasznalo = await _context.Felhasznalok.FindAsync(id);
            if (felhasznalo == null) return NotFound();

            // Entitás eltávolítása a kontextusból (DELETE SQL a SaveChanges-nél)
            _context.Felhasznalok.Remove(felhasznalo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ===== FELHASZNÁLÓ KERESÉSE FELHASZNÁLÓNÉV ALAPJÁN =====
        // GET api/Felhasznalok/nev/{felhasznalonev}
        // Publikus profil megtekintéséhez – pl. "/profil/pistike" típusú útvonalhoz
        [AllowAnonymous]    // Token nélkül is elérhető (publikus profil)
        [HttpGet("nev/{felhasznalonev}")]
        public async Task<IActionResult> GetByUsername(string felhasznalonev)
        {
            var felhasznalo = await _context.Felhasznalok
                .Where(x => x.FelhasznaloNev == felhasznalonev)
                // DTO használata – csak a publikusan megjeleníthető mezők (jelszó hash NINCS benne!)
                .Select(x => new FelhasznaloDTO
                {
                    FelhasznaloID = x.FelhasznaloID,
                    FelhasznaloNev = x.FelhasznaloNev,
                    TeljesNev = x.TeljesNev,
                    Email = x.Email,
                    SzuletesiDatum = x.SzuletesiDatum,
                    Bio = x.Bio,
                    Helyszin = x.Helyszin,
                    Klub = x.Klub,
                    AvatarUrl = x.AvatarUrl,
                    CoverUrl = x.CoverUrl
                })
                .FirstOrDefaultAsync(); // Első találat vagy null

            if (felhasznalo == null)
                return NotFound();

            return Ok(felhasznalo);
        }

        // ===== PROFIL STATISZTIKÁK =====
        // GET api/Felhasznalok/{id}/statisztika
        // Egy felhasználó profiljához tartozó számszerű adatok
        [AllowAnonymous]
        [HttpGet("{id}/statisztika")]
        public async Task<IActionResult> GetProfilStatisztika(int id)
        {
            // Négy külön lekérdezés a különböző statisztikákhoz
            var posztok = await _context.Bejegyzesek.CountAsync(x => x.FelhasznaloID == id);
            var kommentek = await _context.Kommentek.CountAsync(x => x.FelhasznaloID == id);
            var kovetok = await _context.Kovetesek.CountAsync(x => x.KovetettFelhasznaloID == id);
            var kovetettek = await _context.Kovetesek.CountAsync(x => x.KovetoFelhasznaloID == id);

            // Anonim objektumot adunk vissza (nem DTO, hanem menet közben létrehozott névtelen típus)
            // JSON-ná alakítva: { "posztok": 5, "kommentek": 12, "kovetok": 3, "kovetettek": 8 }
            return Ok(new
            {
                posztok,
                kommentek,
                kovetok,
                kovetettek
            });
        }

        // ===== EGY FELHASZNÁLÓ BEJEGYZÉSEI =====
        // GET api/Felhasznalok/{id}/posztok
        // Profiloldalon megjelenő bejegyzések listája
        [AllowAnonymous]
        [HttpGet("{id}/posztok")]
        public async Task<IActionResult> GetFelhasznaloPosztok(int id)
        {
            // Bejegyzések lekérdezése a kapcsolódó entitásokkal (Include = SQL JOIN)
            var lista = await _context.Bejegyzesek
                .Include(b => b.Felhasznalo)        // Bejegyzés szerzőjének adatai
                .Include(b => b.Spot)               // Kapcsolódó spot (helyszín) adatai
                .Include(b => b.Kommentek)          // Kommentek betöltése (a Count-hoz kell)
                .Where(b => b.FelhasznaloID == id)  // Csak az adott felhasználó bejegyzései
                .OrderByDescending(b => b.Letrehozva) // Legújabb elöl
                .ToListAsync();

            // Memóriában mappelés DTO-ba (nem az adatbázisban, mert URL-építés van benne)
            var result = lista.Select(b => new BejegyzesListaDto
            {
                BejegyzesID = b.BejegyzesID,
                Cim = b.Cim,
                Tartalom = b.Tartalom,
                // Kép URL kezelés:
                // - Ha üres/null → null
                // - Ha már teljes URL (http://...) → meghagyjuk
                // - Ha relatív útvonal (/uploads/...) → teljes URL-t építünk belőle
                KepUrl = string.IsNullOrWhiteSpace(b.KepUrl)
                    ? null
                    : (Uri.TryCreate(b.KepUrl, UriKind.Absolute, out _)
                        ? b.KepUrl // Már abszolút URL → meghagyjuk
                        : $"{Request.Scheme}://{Request.Host}{b.KepUrl}"), // Relatív → abszolúttá alakítjuk
                Letrehozva = b.Letrehozva,
                FelhasznaloID = b.FelhasznaloID,
                FelhasznaloNev = b.Felhasznalo?.FelhasznaloNev, // "?." – null-safe: ha nincs Felhasznalo, nem dob hibát
                TeljesNev = b.Felhasznalo?.TeljesNev,
                // Avatar URL-nél ugyanaz a relatív/abszolút logika
                AvatarUrl = string.IsNullOrWhiteSpace(b.Felhasznalo?.AvatarUrl)
                    ? null
                    : (Uri.TryCreate(b.Felhasznalo.AvatarUrl, UriKind.Absolute, out _)
                        ? b.Felhasznalo.AvatarUrl
                        : $"{Request.Scheme}://{Request.Host}{b.Felhasznalo.AvatarUrl}"),
                SpotID = b.SpotID,
                SpotNev = b.Spot?.Nev,
                SpotSlug = b.Spot?.Slug,
                KommentekSzama = b.Kommentek.Count  // A betöltött kommentek száma
            }).ToList();

            return Ok(result);
        }

        // ===== KÖVETŐK LISTÁZÁSA =====
        // GET api/Felhasznalok/{id}/kovetok
        // Kik követik az adott felhasználót
        [AllowAnonymous]
        [HttpGet("{id}/kovetok")]
        public async Task<IActionResult> GetKovetok(int id)
        {
            var result = await _context.Kovetesek
                // Szűrés: ahol a KÖVETETT személy az adott felhasználó
                .Where(k => k.KovetettFelhasznaloID == id)
                // Join: a Kovetesek táblát összekapcsoljuk a Felhasznalok táblával
                // hogy megkapjuk a KÖVETŐ felhasználók adatait
                .Join(
                    _context.Felhasznalok,          // Melyik táblával joinolunk
                    k => k.KovetoFelhasznaloID,     // Követés tábla kulcsa (ki a követő)
                    f => f.FelhasznaloID,           // Felhasználó tábla kulcsa
                    (k, f) => new FelhasznaloDTO    // Az eredmény: a követő felhasználó adatai DTO-ban
                    {
                        FelhasznaloID = f.FelhasznaloID,
                        FelhasznaloNev = f.FelhasznaloNev,
                        TeljesNev = f.TeljesNev,
                        Email = f.Email,
                        SzuletesiDatum = f.SzuletesiDatum,
                        Bio = f.Bio,
                        Helyszin = f.Helyszin,
                        Klub = f.Klub,
                        AvatarUrl = f.AvatarUrl,
                        CoverUrl = f.CoverUrl
                    })
                .ToListAsync();

            return Ok(result);
        }

        // ===== KÖVETETTEK LISTÁZÁSA =====
        // GET api/Felhasznalok/{id}/kovetettek
        // Kiket követ az adott felhasználó
        [AllowAnonymous]
        [HttpGet("{id}/kovetettek")]
        public async Task<IActionResult> GetKovetettek(int id)
        {
            var result = await _context.Kovetesek
                // Szűrés: ahol a KÖVETŐ az adott felhasználó
                .Where(k => k.KovetoFelhasznaloID == id)
                // Join: a követett felhasználók adatainak lekérdezése
                .Join(
                    _context.Felhasznalok,
                    k => k.KovetettFelhasznaloID,   // Most a KÖVETETT ID-val joinolunk (fordítva, mint fent)
                    f => f.FelhasznaloID,
                    (k, f) => new FelhasznaloDTO
                    {
                        FelhasznaloID = f.FelhasznaloID,
                        FelhasznaloNev = f.FelhasznaloNev,
                        TeljesNev = f.TeljesNev,
                        Email = f.Email,
                        SzuletesiDatum = f.SzuletesiDatum,
                        Bio = f.Bio,
                        Helyszin = f.Helyszin,
                        Klub = f.Klub,
                        AvatarUrl = f.AvatarUrl,
                        CoverUrl = f.CoverUrl
                    })
                .ToListAsync();

            return Ok(result);
        }
    }

}
