using ErnyosKozoApi.Data;
using ErnyosKozoApi.Dtos;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzarnysegedShared.Dtos;
using SzarnysegedShared.DTOs;

namespace ErnyosKozoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HirekController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HirekController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Hirek
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hirek = await _context.Hirek
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

            return Ok(hirek);
        }

        // GET: api/Hirek/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var hir = await _context.Hirek
                .Where(h => h.HirID == id)
                .Select(h => new HirDto
                {
                    HirID = h.HirID,
                    Cim = h.Cim,
                    Tartalom = h.Tartalom,
                    KepUrl = h.KepUrl,
                    Kategoria = h.Kategoria,
                    Datum = h.Datum
                })
                .FirstOrDefaultAsync();

            if (hir == null) return NotFound();

            return Ok(hir);
        }

        // POST: api/Hirek
        [HttpPost]
        public async Task<IActionResult> Create(CreateHirDto dto)
        {
            var hir = new Hir
            {
                Cim = dto.Cim,
                Tartalom = dto.Tartalom,
                KepUrl = dto.KepUrl,
                Kategoria = dto.Kategoria,
                Datum = dto.Datum
            };

            _context.Hirek.Add(hir);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = hir.HirID }, hir);
        }

        // PUT: api/Hirek/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateHirDto dto)
        {
            var hir = await _context.Hirek.FindAsync(id);
            if (hir == null) return NotFound();

            hir.Cim = dto.Cim;
            hir.Tartalom = dto.Tartalom;
            hir.KepUrl = dto.KepUrl;
            hir.Kategoria = dto.Kategoria;
            hir.Datum = dto.Datum;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Hirek/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var hir = await _context.Hirek.FindAsync(id);
            if (hir == null) return NotFound();

            _context.Hirek.Remove(hir);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}