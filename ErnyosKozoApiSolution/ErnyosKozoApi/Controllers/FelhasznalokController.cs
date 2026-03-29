using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;
using SzarnysegedShared.DTOs.ForumDTOs;

namespace ErnyosKozoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FelhasznalokController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FelhasznalokController(AppDbContext context)
        {
            _context = context;
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetFelhasznalok()
        {
            return Ok(await _context.Felhasznalok.ToListAsync());
        }

        // READ ONE
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFelhasznalo(int id)
        {
            var felhasznalo = await _context.Felhasznalok.FindAsync(id);
            if (felhasznalo == null) return NotFound();
            return Ok(felhasznalo);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> CreateFelhasznalo(Felhasznalo felhasznalo)
        {
            _context.Felhasznalok.Add(felhasznalo);
            await _context.SaveChangesAsync();
            return Ok(felhasznalo);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFelhasznalo(int id, Felhasznalo felhasznalo)
        {
            if (id != felhasznalo.FelhasznaloID) return BadRequest();

            _context.Entry(felhasznalo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFelhasznalo(int id)
        {
            var felhasznalo = await _context.Felhasznalok.FindAsync(id);
            if (felhasznalo == null) return NotFound();

            _context.Felhasznalok.Remove(felhasznalo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("nev/{felhasznalonev}")]
        public async Task<IActionResult> GetByUsername(string felhasznalonev)
        {
            var felhasznalo = await _context.Felhasznalok
                .Where(x => x.FelhasznaloNev == felhasznalonev)
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
                .FirstOrDefaultAsync();

            if (felhasznalo == null)
                return NotFound();

            return Ok(felhasznalo);
        }

        [AllowAnonymous]
        [HttpGet("{id}/statisztika")]
        public async Task<IActionResult> GetProfilStatisztika(int id)
        {
            var posztok = await _context.Bejegyzesek.CountAsync(x => x.FelhasznaloID == id);
            var kommentek = await _context.Kommentek.CountAsync(x => x.FelhasznaloID == id);
            var kovetok = await _context.Kovetesek.CountAsync(x => x.KovetettFelhasznaloID == id);
            var kovetettek = await _context.Kovetesek.CountAsync(x => x.KovetoFelhasznaloID == id);

            return Ok(new
            {
                posztok,
                kommentek,
                kovetok,
                kovetettek
            });
        }

        [AllowAnonymous]
        [HttpGet("{id}/posztok")]
        public async Task<IActionResult> GetFelhasznaloPosztok(int id)
        {
            var lista = await _context.Bejegyzesek
                .Include(b => b.Felhasznalo)
                .Include(b => b.Spot)
                .Include(b => b.Kommentek)
                .Where(b => b.FelhasznaloID == id)
                .OrderByDescending(b => b.Letrehozva)
                .ToListAsync();

            var result = lista.Select(b => new BejegyzesListaDto
            {
                BejegyzesID = b.BejegyzesID,
                Cim = b.Cim,
                Tartalom = b.Tartalom,
                KepUrl = string.IsNullOrWhiteSpace(b.KepUrl)
                    ? null
                    : (Uri.TryCreate(b.KepUrl, UriKind.Absolute, out _)
                        ? b.KepUrl
                        : $"{Request.Scheme}://{Request.Host}{b.KepUrl}"),
                Letrehozva = b.Letrehozva,
                FelhasznaloID = b.FelhasznaloID,
                FelhasznaloNev = b.Felhasznalo?.FelhasznaloNev,
                TeljesNev = b.Felhasznalo?.TeljesNev,
                AvatarUrl = string.IsNullOrWhiteSpace(b.Felhasznalo?.AvatarUrl)
                    ? null
                    : (Uri.TryCreate(b.Felhasznalo.AvatarUrl, UriKind.Absolute, out _)
                        ? b.Felhasznalo.AvatarUrl
                        : $"{Request.Scheme}://{Request.Host}{b.Felhasznalo.AvatarUrl}"),
                SpotID = b.SpotID,
                SpotNev = b.Spot?.Nev,
                SpotSlug = b.Spot?.Slug,
                KommentekSzama = b.Kommentek.Count
            }).ToList();

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id}/kovetok")]
        public async Task<IActionResult> GetKovetok(int id)
        {
            var result = await _context.Kovetesek
                .Where(k => k.KovetettFelhasznaloID == id)
                .Join(
                    _context.Felhasznalok,
                    k => k.KovetoFelhasznaloID,
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

        [AllowAnonymous]
        [HttpGet("{id}/kovetettek")]
        public async Task<IActionResult> GetKovetettek(int id)
        {
            var result = await _context.Kovetesek
                .Where(k => k.KovetoFelhasznaloID == id)
                .Join(
                    _context.Felhasznalok,
                    k => k.KovetettFelhasznaloID,
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
