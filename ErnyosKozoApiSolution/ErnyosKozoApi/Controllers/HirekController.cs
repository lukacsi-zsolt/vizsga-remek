using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzarnysegedShared.DTOs.HirDTOs;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]             // API viselkedés: automatikus model validáció
    [Route("api/[controller]")] // Útvonal: api/Hirek
    // NINCS osztályszintű [Authorize] → az olvasási végpontok publikusak,
    // az írás/módosítás/törlés végpontokon egyenként van [Authorize]
    public class HirekController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Konstruktor – DI-ból kapjuk az adatbázis kontextust
        public HirekController(AppDbContext context)
        {
            _context = context;
        }

        // ===== ÖSSZES HÍR LISTÁZÁSA =====
        // GET api/Hirek
        // Publikus végpont – bárki (bejelentkezés nélkül is) megtekintheti a híreket
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hirek = await _context.Hirek
                .OrderByDescending(h => h.Datum)    // Legfrissebb hírek elöl
                // Select + DTO: csak a szükséges mezők kerülnek a válaszba
                .Select(h => new HirDto
                {
                    HirID = h.HirID,
                    Cim = h.Cim,
                    Tartalom = h.Tartalom,
                    KepUrl = h.KepUrl,
                    Kategoria = h.Kategoria,
                    Datum = h.Datum
                })
                .ToListAsync();

            return Ok(hirek);   // 200 OK + JSON tömb
        }

        // ===== EGY HÍR LEKÉRDEZÉSE =====
        // GET api/Hirek/{id}
        // Publikus – egy konkrét hír részletei
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var hir = await _context.Hirek
                .Where(h => h.HirID == id)  // Szűrés ID alapján
                .Select(h => new HirDto
                {
                    HirID = h.HirID,
                    Cim = h.Cim,
                    Tartalom = h.Tartalom,
                    KepUrl = h.KepUrl,
                    Kategoria = h.Kategoria,
                    Datum = h.Datum
                })
                .FirstOrDefaultAsync(); // Első találat vagy null

            if (hir == null) return NotFound(); // 404 ha nem létezik

            return Ok(hir);
        }

        // ===== ÚJ HÍR LÉTREHOZÁSA =====
        // POST api/Hirek
        // Csak bejelentkezett admin hozhat létre hírt
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateHirDto dto)
        {
            // Admin jogosultság ellenőrzése
            if (!User.IsAdmin())
                return Forbid();    // 403 – bejelentkezett, de nem admin

            // Új Hir entitás létrehozása a DTO adataiból
            var hir = new Hir
            {
                Cim = dto.Cim,
                Tartalom = dto.Tartalom,
                KepUrl = dto.KepUrl,
                Kategoria = dto.Kategoria,
                // Ha a kliens nem küldött dátumot (default érték) → az aktuális UTC időt használjuk
                Datum = dto.Datum == default ? DateTime.UtcNow : dto.Datum
            };

            _context.Hirek.Add(hir);
            await _context.SaveChangesAsync();
            // 201 Created válasz – REST konvenció szerint új erőforrás létrehozásánál
            // CreatedAtAction: beállítja a Location headert az új erőforrás URL-jére
            // nameof(Get) → a "Get" metódus nevét adja string-ként ("Get")
            // new { id = hir.HirID } → az útvonal paraméter az új hír ID-jával
            // hir → a válasz body-jában visszaküldjük a létrehozott entitást

            return CreatedAtAction(nameof(Get), new { id = hir.HirID }, hir);
        }

        // ===== HÍR MÓDOSÍTÁSA =====
        // PUT api/Hirek/{id}
        // Csak admin módosíthat hírt
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateHirDto dto)
        {
            if (!User.IsAdmin())
                return Forbid();

            // Hír keresése az adatbázisban
            var hir = await _context.Hirek.FindAsync(id);
            if (hir == null) return NotFound();

            // Mezők felülírása a DTO adataival
            hir.Cim = dto.Cim;
            hir.Tartalom = dto.Tartalom;
            hir.KepUrl = dto.KepUrl;
            hir.Kategoria = dto.Kategoria;
            hir.Datum = dto.Datum;

            // SaveChangesAsync automatikusan észleli a változásokat (Change Tracking)
            // mert a "hir" entitást FindAsync-kal töltöttük be → az EF Core "tracked" állapotban tartja
            await _context.SaveChangesAsync();

            return NoContent(); // 204 – sikeres módosítás
        }

        // ===== HÍR TÖRLÉSE =====
        // DELETE api/Hirek/{id}
        // Csak admin törölhet hírt
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsAdmin())
                return Forbid();

            var hir = await _context.Hirek.FindAsync(id);
            if (hir == null) return NotFound();

            _context.Hirek.Remove(hir);         // Törlésre jelölés
            await _context.SaveChangesAsync();  // DELETE SQL végrehajtása

            return NoContent(); // 204
        }
    }
}