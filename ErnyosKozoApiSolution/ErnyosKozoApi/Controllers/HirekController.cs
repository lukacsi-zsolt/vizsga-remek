using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzarnysegedShared.DTOs.HirDTOs;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hirek = await _context.Hirek
                .OrderByDescending(h => h.Datum)
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateHirDto dto)
        {
            if (!User.IsAdmin())
                return Forbid();

            var hir = new Hir
            {
                Cim = dto.Cim,
                Tartalom = dto.Tartalom,
                KepUrl = dto.KepUrl,
                Kategoria = dto.Kategoria,
                Datum = dto.Datum == default ? DateTime.UtcNow : dto.Datum
            };

            _context.Hirek.Add(hir);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = hir.HirID }, hir);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateHirDto dto)
        {
            if (!User.IsAdmin())
                return Forbid();

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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsAdmin())
                return Forbid();

            var hir = await _context.Hirek.FindAsync(id);
            if (hir == null) return NotFound();

            _context.Hirek.Remove(hir);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}