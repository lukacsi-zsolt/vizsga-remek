using ErnyosKozoApi.Data;
using ErnyosKozoApi.Helpers;
using ErnyosKozoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// DTO-k (Data Transfer Object-ek) – a kliens felé küldött/fogadott adatszerkezetek
// A DTO-k célja, hogy ne az adatbázis entitásokat küldjük közvetlenül, hanem csak a szükséges mezőket
using SzarnysegedShared.DTOs.AdminDTOs;
using SzarnysegedShared.DTOs.HirDTOs;
using SzarnysegedShared.DTOs.ForumDTOs;

namespace ErnyosKozoApi.Controllers
{
    // ===== OSZTÁLY SZINTŰ ATTRIBÚTUMOK =====
    [Authorize]                                     // Az ÖSSZES végpont csak bejelentkezett (érvényes JWT tokennel rendelkező) felhasználónak érhető el
    [ApiController]                                 // API-specifikus viselkedés: automatikus model validáció, 400-as válasz hibás kérésnél
    [Route("api/[controller]")]                     // Útvonal: api/Admin – a [controller] placeholder az osztály nevéből jön ("Admin"Controller)
    public class AdminController : ControllerBase   // ControllerBase: alap API controller (View nélkül, csak adatot küld vissza)
    {
        // Az adatbázis kontextus DI-ból (Dependency Injection) érkezik
        // readonly: csak a konstruktorban kaphat értéket, utána nem módosítható
        private readonly AppDbContext _context;

        // Konstruktor – a DI container automatikusan beinjektálja az AppDbContext-et
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ===== ADMIN JOGOSULTSÁG ELLENŐRZÉS =====
        // Segédmetódus: minden végpont elején meghívjuk, hogy ellenőrizzük, admin-e a felhasználó
        // Ha admin → null-t ad vissza (nincs hiba, mehet tovább)
        // Ha NEM admin → Forbid() eredményt ad (403 Forbidden válasz)
        private IActionResult? EnsureAdmin()
        {
            // User.IsAdmin() – a ClaimsPrincipalExtensions-ben definiált kiterjesztő metódus
            // A JWT tokenben lévő claims alapján dönti el
            if (!User.IsAdmin())
                return Forbid();

            return null; // null = rendben, a felhasználó admin
        }

        // ===== DASHBOARD STATISZTIKÁK =====
        // GET api/Admin/dashboard
        // Az admin felület főoldalán megjelenő összesítő adatokat adja vissza
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            // Admin ellenőrzés – ha nem admin, azonnal 403-at küld vissza
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            // Az utolsó 7 nap kezdő dátumának kiszámítása (ma + 6 korábbi nap)
            var today = DateTime.UtcNow.Date; // UTC idő használata – szerver-független időzóna
            var start = today.AddDays(-6);

            // Összesített statisztikák lekérdezése az adatbázisból
            var dashboard = new AdminDashboardDto
            {
                UsersCount = await _context.Felhasznalok.CountAsync(),                                  // Összes regisztrált felhasználó
                PostsCount = await _context.Bejegyzesek.CountAsync(),                                   // Összes fórum bejegyzés
                NewsCount = await _context.Hirek.CountAsync(),                                          // Összes hír
                SpotsCount = await _context.Spotok.CountAsync(),                                        // Összes repülős helyszín (spot)
                SpotSuggestionsCount = await _context.SpotJavaslatok.CountAsync(x => !x.Feldolgozva)    // Feldolgozatlan javaslatok száma, csak ahol Feldolgozva == false
            };

            // Napi bontású statisztika az utolsó 7 napra (ciklus minden napra)
            for (var day = start; day <= today; day = day.AddDays(1))
            {
                var next = day.AddDays(1); // A következő nap (intervallum felső határa)

                // Minden napra külön lekérdezzük az adott napon létrehozott elemek számát
                dashboard.Last7Days.Add(new AdminDailyStatDto
                {
                    Label = day.ToString("MM.dd"), // Megjelenítendő dátum formátum (pl. "04.07")
                    // Felhasználók akik ezen a napon regisztráltak (RegDatum >= nap ÉS < következő nap)
                    Users = await _context.Felhasznalok.CountAsync(x => x.RegDatum >= day && x.RegDatum < next),
                    // Bejegyzések amelyek ezen a napon készültek
                    Posts = await _context.Bejegyzesek.CountAsync(x => x.Letrehozva >= day && x.Letrehozva < next),
                    // Hírek amelyek ezen a napon jelentek meg
                    News = await _context.Hirek.CountAsync(x => x.Datum >= day && x.Datum < next),
                    Spots = 0, // Spotoknak nincs létrehozási dátuma, ezért fix 0
                    // Spot javaslatok amelyeket ezen a napon küldtek be
                    Suggestions = await _context.SpotJavaslatok.CountAsync(x => x.Letrehozva >= day && x.Letrehozva < next)
                });
            }

            // 200 OK válasz a dashboard adatokkal (JSON formátumban)
            return Ok(dashboard);
        }

        // ===== FELHASZNÁLÓK LISTÁZÁSA =====
        // GET api/Admin/users
        // Az összes felhasználó adatait adja vissza az admin számára
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var users = await _context.Felhasznalok
                .OrderByDescending(x => x.RegDatum) // Legújabb regisztrációk elöl
                                                    // Select + DTO: csak a szükséges mezőket küldjük a kliensnek (nem az egész entitást)
                                                    // Ez biztonságosabb (pl. jelszó hash nem megy ki) és hatékonyabb
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
                .ToListAsync();         // Aszinkron végrehajtás – nem blokkolja a szálat

            return Ok(users);
        }

        // ===== FELHASZNÁLÓ MÓDOSÍTÁSA =====
        // PUT api/Admin/users/{id}
        // Az admin módosíthatja bármelyik felhasználó adatait
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, AdminUserDto dto)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            // Felhasználó keresése az ID alapján
            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();    // 404 ha nem létezik

            // Ellenőrzés: a megadott felhasználónév nem foglalt-e már MÁS felhasználónál
            var usernameExists = await _context.Felhasznalok.AnyAsync(x =>
                x.FelhasznaloNev == dto.FelhasznaloNev &&
                x.FelhasznaloID != id); // Saját magát kizárjuk az ellenőrzésből

            if (usernameExists)
                return BadRequest("Ez a felhasználónév már foglalt."); // 400-as hibaüzenet

            // Felhasználó mezőinek frissítése a DTO adataival
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

            // Változások mentése az adatbázisba
            await _context.SaveChangesAsync();
            return NoContent(); // 204 No Content – sikeres módosítás, nincs visszaküldendő adat
        }

        // ===== AVATAR ELTÁVOLÍTÁSA =====
        // POST api/Admin/users/{id}/remove-avatar
        // Az admin törölheti bármelyik felhasználó profilképét
        [HttpPost("users/{id}/remove-avatar")]
        public async Task<IActionResult> RemoveAvatar(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            user.AvatarUrl = null; // Avatar URL nullázása – a frontend alapértelmezett képet jelenít meg
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ===== BORÍTÓKÉP ELTÁVOLÍTÁSA =====
        // POST api/Admin/users/{id}/remove-cover
        [HttpPost("users/{id}/remove-cover")]
        public async Task<IActionResult> RemoveCover(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            user.CoverUrl = null; // Borítókép URL nullázása
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ===== FELHASZNÁLÓ TÖRLÉSE =====
        // DELETE api/Admin/users/{id}
        // Kaszkád törlés: a felhasználó összes kapcsolódó adatát is törölni kell
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            // Biztonsági ellenőrzés: az admin nem törölheti saját magát
            // A bejelentkezett felhasználó ID-ját a JWT token NameIdentifier claim-jéből olvassuk ki
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserIdClaim, out int currentUserId) && currentUserId == id)
                return BadRequest("Saját admin fiókodat innen nem törölheted.");

            var user = await _context.Felhasznalok.FindAsync(id);
            if (user == null) return NotFound();

            // A felhasználóhoz kapcsolódó összes adat lekérdezése törléshez
            // Ez azért kell, mert az EF Core-ban explicit kell kezelni a kapcsolódó entitásokat
            var userPosts = await _context.Bejegyzesek
                .Where(x => x.FelhasznaloID == id)
                .ToListAsync();

            var userComments = await _context.Kommentek
                .Where(x => x.FelhasznaloID == id)
                .ToListAsync();

            // Követések: ahol ő követ valakit VAGY őt követi valaki
            var userFollows = await _context.Kovetesek
                .Where(x => x.KovetoFelhasznaloID == id || x.KovetettFelhasznaloID == id)
                .ToListAsync();

            var userSuggestions = await _context.SpotJavaslatok
                .Where(x => x.BekuldoFelhasznaloID == id)
                .ToListAsync();

            // Spotok ahol ő a létrehozó – nem töröljük a spotot, csak a létrehozó referenciát nullázzuk
            var createdSpots = await _context.Spotok
                .Where(x => x.LetrehozoFelhasznaloID == id)
                .ToListAsync();

            foreach (var spot in createdSpots)
            {
                spot.LetrehozoFelhasznaloID = null; // A spot megmarad, de már nincs tulajdonosa
            }

            // Kapcsolódó adatok törlése – a sorrend fontos a foreign key megszorítások miatt!
            // Először a "gyerek" entitásokat töröljük (kommentek, bejegyzések), aztán a "szülőt" (felhasználó)
            _context.Kommentek.RemoveRange(userComments);         // Összes komment törlése
            _context.Bejegyzesek.RemoveRange(userPosts);          // Összes bejegyzés törlése
            _context.Kovetesek.RemoveRange(userFollows);          // Összes követés törlése
            _context.SpotJavaslatok.RemoveRange(userSuggestions); // Összes spot javaslat törlése
            _context.Felhasznalok.Remove(user);                   // Végül a felhasználó törlése

            // Minden törlés egy tranzakcióban hajtódik végre – vagy mind sikerül, vagy semmi
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ===== HÍREK LISTÁZÁSA (admin felületre) =====
        // GET api/Admin/news
        [HttpGet("news")]
        public async Task<IActionResult> GetNews()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var news = await _context.Hirek
                .OrderByDescending(x => x.Datum) // Legújabb hírek elöl
                .Select(x => new HirDto          // DTO-ba mappelés
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

        // ===== SPOTOK LISTÁZÁSA =====
        // GET api/Admin/spots
        [HttpGet("spots")]
        public async Task<IActionResult> GetSpots()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            // Itt nem használunk DTO-t, hanem közvetlenül a Spot entitást küldjük vissza
            // Név szerinti ABC sorrendben
            return Ok(await _context.Spotok.OrderBy(x => x.Nev).ToListAsync());
        }

        // ===== SPOT JAVASLATOK LISTÁZÁSA =====
        // GET api/Admin/spot-suggestions
        // A felhasználók által beküldött, még feldolgozatlan spot javaslatokat listázza
        [HttpGet("spot-suggestions")]
        public async Task<IActionResult> GetSpotSuggestions()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var suggestions = await _context.SpotJavaslatok
                .OrderByDescending(x => x.Letrehozva)   // Legújabb javaslatok elöl
                .ToListAsync();

            return Ok(suggestions);
        }

        // ===== SPOT JAVASLAT ELFOGADÁSA =====
        // POST api/Admin/spot-suggestions/{id}/approve
        // A javaslatból valódi Spot entitást hoz létre az adatbázisban
        [HttpPost("spot-suggestions/{id}/approve")]
        public async Task<IActionResult> ApproveSpotSuggestion(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var suggestion = await _context.SpotJavaslatok.FindAsync(id);
            if (suggestion == null) return NotFound();

            // ===== SLUG GENERÁLÁS =====
            // A slug az URL-barát azonosító (pl. "bakony-hegy" a "Bakony Hegy" névből)
            var slug = (suggestion.Nev ?? "spot")
                .Trim()             // Szóközök levágása az elejéről/végéről
                .ToLower()          // Kisbetűsítés
                                    // Magyar ékezetes karakterek cseréje ékezet nélkülire (URL-kompatibilis)
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ö", "o")
                .Replace("ő", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ű", "u")
                .Replace(" ", "-"); // Szóközök kötőjelre cserélése

            // Ha már létezik ilyen slug, egyedi azonosítót fűzünk hozzá (pl. "bakony-hegy-a3f2b1")
            if (await _context.Spotok.AnyAsync(x => x.Slug == slug))
                slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}"; // GUID első 6 karaktere

            // Új Spot entitás létrehozása a javaslat adataiból
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
                Lat = suggestion.Lat,               // GPS koordináta – szélesség
                Lon = suggestion.Lon,               // GPS koordináta – hosszúság
                LetrehozoFelhasznaloID = suggestion.BekuldoFelhasznaloID // Ki küldte be a javaslatot
            };

            // A javaslat feldolgozottnak jelölése (nem töröljük, csak megjelöljük)
            suggestion.Feldolgozva = true;

            // Az új spot hozzáadása az adatbázishoz
            _context.Spotok.Add(spot);
            await _context.SaveChangesAsync();

            // 200 OK – visszaküldjük a létrehozott spot adatait
            return Ok(spot);
        }

        // ===== SPOT JAVASLAT TÖRLÉSE (elutasítás) =====
        // DELETE api/Admin/spot-suggestions/{id}
        [HttpDelete("spot-suggestions/{id}")]
        public async Task<IActionResult> DeleteSpotSuggestion(int id)
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var suggestion = await _context.SpotJavaslatok.FindAsync(id);
            if (suggestion == null) return NotFound();

            _context.SpotJavaslatok.Remove(suggestion); // Javaslat végleges törlése
            await _context.SaveChangesAsync();

            return NoContent(); // 204 – sikeres törlés
        }

        // ===== BEJEGYZÉSEK LISTÁZÁSA =====
        // GET api/Admin/posts
        // Az összes fórum bejegyzés az admin felületre
        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts()
        {
            var guard = EnsureAdmin();
            if (guard != null) return guard;

            var posts = await _context.Bejegyzesek
                // Include: a kapcsolódó Felhasznalo entitást is betölti (JOIN az SQL-ben)
                // Ez kell, mert a DTO-ban a felhasználó nevét és avatarját is visszaadjuk
                .Include(x => x.Felhasznalo)
                .OrderByDescending(x => x.Letrehozva)   // Legújabb bejegyzések elöl
                .Select(x => new BejegyzesListaDto
                {
                    BejegyzesID = x.BejegyzesID,
                    Cim = x.Cim,
                    Tartalom = x.Tartalom,
                    KepUrl = x.KepUrl,
                    Letrehozva = x.Letrehozva,
                    FelhasznaloID = x.FelhasznaloID,
                    // A "!" (null-forgiving operator) jelzi a fordítónak, hogy tudjuk: nem null
                    // (az Include miatt biztosan be van töltve)
                    FelhasznaloNev = x.Felhasznalo!.FelhasznaloNev,
                    TeljesNev = x.Felhasznalo!.TeljesNev,
                    AvatarUrl = x.Felhasznalo!.AvatarUrl,
                    SpotID = x.SpotID,
                    // Feltételes mapping: ha van kapcsolódó Spot, annak a neve/slug-ja, különben null
                    SpotNev = x.Spot != null ? x.Spot.Nev : null,
                    SpotSlug = x.Spot != null ? x.Spot.Slug : null,
                    // A bejegyzéshez tartozó kommentek számát is visszaadjuk
                    KommentekSzama = x.Kommentek.Count
                })
                .ToListAsync();

            return Ok(posts);
        }
    }
}