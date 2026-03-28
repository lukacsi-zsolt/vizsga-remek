using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzarnysegedShared.DTOs.AdminDTOs;
using SzarnysegedShared.DTOs.HirDTOs;
using SzarnysegedShared.DTOs.ForumDTOs;

namespace ErnyosKozoApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? EnsureAdmin()
        {
            if (!User.IsAdmin())
                return Forbid();

            return null;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var today = DateTime.UtcNow.Date;
            var start = today.AddDays(-6);

            var dashboard = new AdminDashboardDto
            {
                UsersCount = await _context.Felhasznalok.CountAsync(),
                PostsCount = await _context.Bejegyzesek.CountAsync(),
                NewsCount = await _context.Hirek.CountAsync(),
                SpotsCount = await _context.Spotok.CountAsync(),
                SpotSuggestionsCount = await _context.SpotJavaslatok.CountAsync(x => !x.Feldolgozva)
            };

            for (var day = start; day <= today; day = day.AddDays(1))
            {
                var next = day.AddDays(1);

                dashboard.Last7Days.Add(new AdminDailyStatDto
                {
                    Label = day.ToString("MM.dd"),
                    Users = await _context.Felhasznalok.CountAsync(x => x.RegDatum >= day && x.RegDatum < next),
                    Posts = await _context.Bejegyzesek.CountAsync(x => x.Letrehozva >= day && x.Letrehozva < next),
                    News = await _context.Hirek.CountAsync(x => x.Datum >= day && x.Datum < next),
                    Spots = 0,
                    Suggestions = await _context.SpotJavaslatok.CountAsync(x => x.Letrehozva >= day && x.Letrehozva < next)
                });
            }

            return Ok(dashboard);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var users = await _context.Felhasznalok
                .OrderByDescending(x => x.RegDatum)
                .Select(x => new AdminUserDto
                {
                    FelhasznaloID = x.FelhasznaloID,
                    FelhasznaloNev = x.FelhasznaloNev,
                    TeljesNev = x.TeljesNev,
                    Email = x.Email,
                    SzuletesiDatum = x.SzuletesiDatum,
                    RegDatum = x.RegDatum,
                    Bio = x.Bio,
                    Helyszin = x.Helyszin,
                    Klub = x.Klub,
                    AvatarUrl = x.AvatarUrl,
                    CoverUrl = x.CoverUrl,
                    IsAdmin = x.IsAdmin
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, AdminUserDto dto)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            var usernameExists = await _context.Felhasznalok.AnyAsync(x =>
                x.FelhasznaloNev == dto.FelhasznaloNev &&
                x.FelhasznaloID != id);

            if (usernameExists)
                return BadRequest("Ez a felhasználónév már foglalt.");

            user.FelhasznaloNev = dto.FelhasznaloNev;
            user.TeljesNev = dto.TeljesNev;
            user.Email = dto.Email;
            user.SzuletesiDatum = dto.SzuletesiDatum;
            user.Bio = dto.Bio;
            user.Helyszin = dto.Helyszin;
            user.Klub = dto.Klub;
            user.AvatarUrl = dto.AvatarUrl;
            user.CoverUrl = dto.CoverUrl;
            user.IsAdmin = dto.IsAdmin;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("users/{id}/remove-avatar")]
        public async Task<IActionResult> RemoveAvatar(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            user.AvatarUrl = null;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("users/{id}/remove-cover")]
        public async Task<IActionResult> RemoveCover(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            user.CoverUrl = null;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            var userComments = await _context.Kommentek.Where(x => x.FelhasznaloID == id).ToListAsync();
            var userPosts = await _context.Bejegyzesek.Where(x => x.FelhasznaloID == id).ToListAsync();
            var follows = await _context.Kovetesek
                .Where(x => x.KovetoFelhasznaloID == id || x.KovetettFelhasznaloID == id)
                .ToListAsync();

            _context.Kommentek.RemoveRange(userComments);
            _context.Bejegyzesek.RemoveRange(userPosts);
            _context.Kovetesek.RemoveRange(follows);
            _context.Felhasznalok.Remove(user);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("news")]
        public async Task<IActionResult> GetNews()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var news = await _context.Hirek
                .OrderByDescending(x => x.Datum)
                .Select(x => new HirDto
                {
                    HirID = x.HirID,
                    Cim = x.Cim,
                    Tartalom = x.Tartalom,
                    KepUrl = x.KepUrl,
                    Kategoria = x.Kategoria,
                    Datum = x.Datum
                })
                .ToListAsync();

            return Ok(news);
        }

        [HttpGet("spots")]
        public async Task<IActionResult> GetSpots()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            return Ok(await _context.Spotok.OrderBy(x => x.Nev).ToListAsync());
        }

        [HttpGet("spot-suggestions")]
        public async Task<IActionResult> GetSpotSuggestions()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var suggestions = await _context.SpotJavaslatok
                .OrderByDescending(x => x.Letrehozva)
                .ToListAsync();

            return Ok(suggestions);
        }

        [HttpPost("spot-suggestions/{id}/approve")]
        public async Task<IActionResult> ApproveSpotSuggestion(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var suggestion = await _context.SpotJavaslatok.FindAsync(id);
            if (suggestion == null) return NotFound();

            var slug = (suggestion.Nev ?? "spot")
                .Trim()
                .ToLower()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ö", "o")
                .Replace("ő", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ű", "u")
                .Replace(" ", "-");

            if (await _context.Spotok.AnyAsync(x => x.Slug == slug))
                slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";

            var spot = new Spot
            {
                Nev = suggestion.Nev,
                Slug = slug,
                Orszag = suggestion.Orszag,
                Megye = suggestion.Megye,
                HelyLeiras = suggestion.HelyLeiras,
                Magassag = suggestion.Magassag,
                AtlagSzel = suggestion.AtlagSzel,
                Szabalyok = suggestion.Szabalyok,
                Lat = suggestion.Lat,
                Lon = suggestion.Lon,
                LetrehozoFelhasznaloID = suggestion.BekuldoFelhasznaloID
            };

            suggestion.Feldolgozva = true;

            _context.Spotok.Add(spot);
            await _context.SaveChangesAsync();

            return Ok(spot);
        }

        [HttpDelete("spot-suggestions/{id}")]
        public async Task<IActionResult> DeleteSpotSuggestion(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var suggestion = await _context.SpotJavaslatok.FindAsync(id);
            if (suggestion == null) return NotFound();

            _context.SpotJavaslatok.Remove(suggestion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var posts = await _context.Bejegyzesek
                .Include(x => x.Felhasznalo)
                .OrderByDescending(x => x.Letrehozva)
                .Select(x => new BejegyzesListaDto
                {
                    BejegyzesID = x.BejegyzesID,
                    Cim = x.Cim,
                    Tartalom = x.Tartalom,
                    KepUrl = x.KepUrl,
                    Letrehozva = x.Letrehozva,
                    FelhasznaloID = x.FelhasznaloID,
                    FelhasznaloNev = x.Felhasznalo!.FelhasznaloNev,
                    TeljesNev = x.Felhasznalo!.TeljesNev,
                    AvatarUrl = x.Felhasznalo!.AvatarUrl,
                    SpotID = x.SpotID,
                    SpotNev = x.Spot != null ? x.Spot.Nev : null,
                    SpotSlug = x.Spot != null ? x.Spot.Slug : null,
                    KommentekSzama = x.Kommentek.Count
                })
                .ToListAsync();

            return Ok(posts);
        }
    }
}