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
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ForumController(AppDbContext context)
        {
            _context = context;
        }

        private string? ToAbsoluteUrl(string? relativeOrAbsoluteUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl))
                return null;

            if (Uri.TryCreate(relativeOrAbsoluteUrl, UriKind.Absolute, out _))
                return relativeOrAbsoluteUrl;

            return $"{Request.Scheme}://{Request.Host}{relativeOrAbsoluteUrl}";
        }

        [HttpGet("bejegyzesek")]
        public async Task<ActionResult<List<BejegyzesListaDto>>> GetBejegyzesek()
        {
            var lista = await _context.Bejegyzesek
                .Include(b => b.Felhasznalo)
                .Include(b => b.Spot)
                .Include(b => b.Kommentek)
                .OrderByDescending(b => b.Letrehozva)
                .ToListAsync();

            var result = lista.Select(b => new BejegyzesListaDto
            {
                BejegyzesID = b.BejegyzesID,
                Cim = b.Cim,
                Tartalom = b.Tartalom,
                KepUrl = ToAbsoluteUrl(b.KepUrl),
                Letrehozva = b.Letrehozva,
                FelhasznaloID = b.FelhasznaloID,
                FelhasznaloNev = b.Felhasznalo?.FelhasznaloNev,
                TeljesNev = b.Felhasznalo?.TeljesNev,
                AvatarUrl = ToAbsoluteUrl(b.Felhasznalo?.AvatarUrl),
                SpotID = b.SpotID,
                SpotNev = b.Spot?.Nev,
                SpotSlug = b.Spot?.Slug,
                KommentekSzama = b.Kommentek.Count
            }).ToList();

            return Ok(result);
        }

        [HttpGet("bejegyzesek/{id}")]
        public async Task<ActionResult<BejegyzesListaDto>> GetBejegyzes(int id)
        {
            var x = await _context.Bejegyzesek
                .Include(b => b.Felhasznalo)
                .Include(b => b.Spot)
                .Include(b => b.Kommentek)
                .FirstOrDefaultAsync(b => b.BejegyzesID == id);

            if (x == null)
                return NotFound();

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

        [Authorize]
        [HttpPost("bejegyzesek")]
        public async Task<IActionResult> CreateBejegyzes(BejegyzesLetrehozasDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var bejegyzes = new Bejegyzes
            {
                FelhasznaloID = userId,
                Cim = dto.Cim,
                Tartalom = dto.Tartalom,
                KepUrl = string.IsNullOrWhiteSpace(dto.KepUrl) ? null : dto.KepUrl,
                SpotID = dto.SpotID,
                Letrehozva = DateTime.UtcNow
            };

            _context.Bejegyzesek.Add(bejegyzes);
            await _context.SaveChangesAsync();

            return Ok(bejegyzes.BejegyzesID);
        }

        [HttpGet("bejegyzesek/{id}/kommentek")]
        public async Task<ActionResult<List<KommentDto>>> GetKommentek(int id)
        {
            var kommentek = await _context.Kommentek
                .Include(k => k.Felhasznalo)
                .Where(k => k.BejegyzesID == id)
                .OrderBy(k => k.Letrehozva)
                .ToListAsync();

            List<KommentDto> BuildTree(int? szuloId)
            {
                return kommentek
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
                        Valaszok = BuildTree(k.KommentID)
                    })
                    .ToList();
            }

            return Ok(BuildTree(null));
        }

        [Authorize]
        [HttpPost("kommentek")]
        public async Task<IActionResult> CreateKomment(KommentLetrehozasDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var komment = new Komment
            {
                BejegyzesID = dto.BejegyzesID,
                SzuloKommentID = dto.SzuloKommentID,
                FelhasznaloID = userId,
                Tartalom = dto.Tartalom,
                Letrehozva = DateTime.UtcNow
            };

            _context.Kommentek.Add(komment);
            await _context.SaveChangesAsync();

            return Ok(komment.KommentID);
        }

        [Authorize]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadForumImage(IFormFile file)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out _))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("Nincs fájl feltöltve.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Csak .jpg, .jpeg, .png vagy .webp fájl tölthető fel.");

            if (file.Length > 8 * 1024 * 1024)
                return BadRequest("A fájl túl nagy. Maximum 8 MB.");

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "forum");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/forum/{fileName}";
            return Ok(new { imageUrl });
        }
        [Authorize]
        [HttpDelete("bejegyzesek/{id}")]
        public async Task<IActionResult> DeleteBejegyzes(int id)
        {
            if (!User.IsAdmin())
                return Forbid();

            var bejegyzes = await _context.Bejegyzesek.FindAsync(id);
            if (bejegyzes == null)
                return NotFound();

            _context.Bejegyzesek.Remove(bejegyzes);
            await _context.SaveChangesAsync();

            return NoContent();
        }

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