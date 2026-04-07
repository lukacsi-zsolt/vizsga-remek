using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]             // API viselkedés: automatikus model validáció
    [Route("api/[controller]")] // Útvonal: api/Utvonalak
    // NINCS [Authorize] → az összes végpont publikus (nincs jogosultságkezelés)
    // Ez egy egyszerű, "nyers" CRUD controller – vizsgán tökéletes alappélda
    public class UtvonalakController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtvonalakController(AppDbContext context)
        {
            _context = context;
        }

        // ===== READ ALL – Összes útvonal lekérdezése =====
        // GET api/Utvonalak
        // Expression-bodied metódus (=>) – egysoros rövidített szintaxis
        // Ugyanaz, mint: { return Ok(await _context.Utvonalak.ToListAsync()); }
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _context.Utvonalak.ToListAsync());

        // ===== READ ONE – Egy útvonal lekérdezése ID alapján =====
        // GET api/Utvonalak/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // FindAsync: elsődleges kulcs (PK) alapján keres – az EF Core cache-ből is visszaadhatja
            var utvonal = await _context.Utvonalak.FindAsync(id);
            if (utvonal == null) return NotFound(); // 404 ha nem létezik
            return Ok(utvonal); // 200 OK + az útvonal JSON-ként
        }

        // ===== CREATE – Új útvonal létrehozása =====
        // POST api/Utvonalak
        [HttpPost]
        public async Task<IActionResult> Create(Utvonal utvonal)
        {
            // Az entitás hozzáadása a kontextushoz (INSERT előkészítése)
            _context.Utvonalak.Add(utvonal);
            // SaveChangesAsync végrehajtja az INSERT SQL-t és az adatbázis generálja az ID-t
            await _context.SaveChangesAsync();
            return Ok(utvonal); // Visszaadjuk az entitást az adatbázis által generált ID-val
        }

        // ===== UPDATE – Útvonal módosítása =====
        // PUT api/Utvonalak/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Utvonal utvonal)
        {
            // URL és body ID egyezésének ellenőrzése – biztonsági okokból
            if (id != utvonal.UtvonalID) return BadRequest();
            // Az entitás "Modified" állapotra állítása
            // Mivel a klienstől érkezett (nem FindAsync-kal töltöttük be),
            // manuálisan kell jelezni az EF Core-nak, hogy frissítse az összes mezőt
            _context.Entry(utvonal).State = EntityState.Modified;
            await _context.SaveChangesAsync();  // UPDATE SQL végrehajtása
            return NoContent();     // 204 – sikeres módosítás, nincs visszaküldendő adat
        }

        // ===== DELETE – Útvonal törlése =====
        // DELETE api/Utvonalak/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Először megkeressük az entitást (kell a Remove-hoz)
            var utvonal = await _context.Utvonalak.FindAsync(id);
            if (utvonal == null) return NotFound();

            // Törlésre jelölés + mentés → DELETE SQL
            _context.Utvonalak.Remove(utvonal);
            await _context.SaveChangesAsync();
            return NoContent(); // 204
        }
    }
}
