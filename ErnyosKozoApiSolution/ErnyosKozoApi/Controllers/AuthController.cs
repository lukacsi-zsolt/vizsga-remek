using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ErnyosKozoApi.Controllers
{
    [Authorize]             // Alapértelmezetten minden végpont bejelentkezést igényel
    [ApiController]         // API viselkedés: auto validáció, auto 400-as válasz hibás modellnél
    [Route("api/[controller]")] // Útvonal: api/Auth
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;                 // Adatbázis kontextus
        private readonly IConfiguration _config;                // Konfiguráció (appsettings.json elérése)
        private readonly IPasswordHasher<Felhasznalo> _hasher;  // Jelszó hashelő szolgáltatás

        // Konstruktor – DI-ból kapjuk a kontextust és a konfigurációt
        // A PasswordHasher-t itt manuálisan példányosítjuk (nem DI-ból)
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<Felhasznalo>(); // ASP.NET Identity beépített hashelő (bcrypt-szerű)
        }

        // ===== BEJELENTKEZÉS =====
        // POST api/Auth/login
        // [AllowAnonymous] – felülírja az osztályszintű [Authorize]-t, tehát token nélkül is elérhető
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            // 1. Felhasználó keresése felhasználónév alapján
            var user = await _context.Felhasznalok
                .FirstOrDefaultAsync(x => x.FelhasznaloNev == model.Username);

            // Ha nem létezik ilyen felhasználó → 401 Unauthorized
            if (user == null)
                return Unauthorized();

            // 2. Jelszó ellenőrzés – a tárolt hash-t hasonlítjuk össze a megadott jelszóval
            // A PasswordHasher automatikusan kezeli a sózást (salt) és a hash algoritmust
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized();  // Hibás jelszó → 401

            // 3. JWT token claim-jeinek összeállítása
            // A claim-ek a tokenbe ágyazott felhasználói adatok, amelyeket a szerver minden kérésnél kiolvashat
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FelhasznaloNev ?? string.Empty),        // Felhasználónév
                new Claim(ClaimTypes.NameIdentifier, user.FelhasznaloID.ToString()),    // Felhasználó egyedi ID-ja
                new Claim("isAdmin", user.IsAdmin.ToString())                           // Admin jogosultság (egyedi claim)
            };

            // 4. Aláíró kulcs létrehozása – ugyanaz a kulcs, mint a Program.cs-ben a validáláshoz
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!) // "!" – null-forgiving: tudjuk, hogy létezik a kulcs
            );

            // Aláírási beállítások: HMAC-SHA256 algoritmus (szimmetrikus, gyors, biztonságos)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. JWT token létrehozása
            var token = new JwtSecurityToken(
                claims: claims,                     // A tokenbe ágyazott adatok
                expires: DateTime.Now.AddHours(6),  // Lejárati idő: 6 óra múlva
                signingCredentials: creds           // Aláírás
            );

            // 6. A token string formátumban való visszaküldése a kliensnek
            // A kliens ezt tárolja (pl. localStorage) és minden kérésnél az Authorization headerben küldi
            return Ok(new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token) // Token objektum → Base64 string
            });
        }

        // ===== REGISZTRÁCIÓ =====
        // POST api/Auth/register
        [HttpPost("register")]
        [AllowAnonymous]        // Regisztrálni token nélkül is lehet
        public async Task<IActionResult> Register(CreateFelhasznaloDto dto)
        {
            // Ellenőrzés: a felhasználónév még nem foglalt-e
            if (await _context.Felhasznalok.AnyAsync(x => x.FelhasznaloNev == dto.FelhasznaloNev))
                return BadRequest("Felhasználónév foglalt");

            // Új felhasználó entitás létrehozása a DTO adataiból
            var user = new Felhasznalo
            {
                FelhasznaloNev = dto.FelhasznaloNev,
                TeljesNev = dto.TeljesNev,
                Email = dto.Email,
                SzuletesiDatum = dto.SzuletesiDatum,
                RegDatum = DateTime.UtcNow,             // Regisztráció időpontja UTC-ben
                IsAdmin = false                         // Új felhasználó sosem admin alapértelmezetten
            };

            // Jelszó hashelése – a nyers jelszót SOHA nem tároljuk, csak a hash-t
            // A HashPassword automatikusan sóz (salt) is
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            // Felhasználó hozzáadása az adatbázishoz és mentés
            _context.Felhasznalok.Add(user);
            await _context.SaveChangesAsync();

            return Ok();    // 200 OK – sikeres regisztráció
        }

        // ===== BEJELENTKEZETT FELHASZNÁLÓ ADATAI =====
        // GET api/Auth/me
        // Nincs [AllowAnonymous], tehát az osztályszintű [Authorize] érvényes → token kötelező
        [HttpGet("me")]
        public async Task<ActionResult<FelhasznaloDTO>> Me()
        {   // A bejelentkezett felhasználó ID-jának kiolvasása a JWT token claim-jéből
            // A User property a ControllerBase-ből jön, a middleware automatikusan kitölti a token alapján
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ha nincs NameIdentifier claim → a token érvénytelen vagy hiányzik
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            // String → int konverzió (a claim mindig string)
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            // Felhasználó lekérdezése és DTO-ba mappelése
            var user = await _context.Felhasznalok
                .Where(x => x.FelhasznaloID == userId)
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
                    CoverUrl = x.CoverUrl,
                    IsAdmin = x.IsAdmin
                })
                .FirstOrDefaultAsync(); // Az első (és egyetlen) találat, vagy null

            if (user == null)
                return NotFound(); // A token érvényes, de a felhasználó már nem létezik az adatbázisban

            return Ok(user);
        }

        // ===== PROFIL MÓDOSÍTÁS =====
        // PUT api/Auth/profile
        // A bejelentkezett felhasználó saját profiljának szerkesztése
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateFelhasznaloDto dto)
        {
            // Bejelentkezett felhasználó azonosítása a tokenből
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Felhasznalok.FirstOrDefaultAsync(x => x.FelhasznaloID == userId);

            if (user == null)
                return NotFound();

            // Felhasználónév egyediségének ellenőrzése (saját magát kizárva)
            var usernameExists = await _context.Felhasznalok.AnyAsync(x =>
                x.FelhasznaloNev == dto.FelhasznaloNev &&
                x.FelhasznaloID != userId);

            if (usernameExists)
                return BadRequest("Ez a felhasználónév már foglalt.");

            // Alap mezők frissítése
            user.FelhasznaloNev = dto.FelhasznaloNev;
            user.TeljesNev = dto.TeljesNev;
            user.Email = dto.Email;
            user.SzuletesiDatum = dto.SzuletesiDatum;
            user.Bio = dto.Bio;
            user.Helyszin = dto.Helyszin;
            user.Klub = dto.Klub;

            // ===== AVATAR URL KEZELÉS =====
            // Háromféle eset:
            // 1. null → avatar törlése (alapértelmezettre visszaállítás)
            if (dto.AvatarUrl == null)
                user.AvatarUrl = null;

            // 2. nem üres string → új avatar beállítása, teljes URL összeállítása
            //    Request.Scheme = "https", Request.Host = "localhost:5001" (vagy a domain)
            else if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
                user.AvatarUrl = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{dto.AvatarUrl}";
            // 3. üres string → nem változtatunk (implicit: nem csinálunk semmit)

            // Ugyanez a logika a borítóképre
            if (dto.CoverUrl == null)
                user.CoverUrl = null;
            else if (!string.IsNullOrWhiteSpace(dto.CoverUrl))
                user.CoverUrl = $"{Request.Scheme}://{Request.Host}/uploads/covers/{dto.CoverUrl}";

            await _context.SaveChangesAsync();

            return Ok();
        }

        // ===== AVATAR FELTÖLTÉS =====
        // POST api/Auth/upload-avatar
        // Fájl feltöltése a szerverre (wwwroot/uploads/avatars mappába)
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            // IFormFile: az ASP.NET Core beépített típusa HTTP fájl feltöltéshez
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            // A célmappa elérési útja – a wwwroot a statikus fájlok gyökere
            var folder = Path.Combine("wwwroot", "uploads", "avatars");
            // Ha még nem létezik a mappa, létrehozzuk
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Egyedi fájlnév generálása GUID-dal – elkerüli a névütközést
            // Az eredeti kiterjesztést megtartjuk (pl. .jpg, .png)
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            // Fájl kiírása a lemezre
            // using: a stream automatikusan bezáródik és felszabadul a blokk végén
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream); // Aszinkron másolás – nem blokkolja a szálat

            // Csak a fájlnevet küldjük vissza – a teljes URL-t a profil frissítésnél állítjuk össze
            return Ok(new { imageUrl = fileName });
        }

        // ===== BORÍTÓKÉP FELTÖLTÉS =====
        // POST api/Auth/upload-cover
        // Ugyanaz a logika, mint az avatar feltöltésnél, csak más mappába ment
        [HttpPost("upload-cover")]
        public async Task<IActionResult> UploadCover(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            var folder = Path.Combine("wwwroot", "uploads", "covers");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new { imageUrl = fileName });
        }
    }
}