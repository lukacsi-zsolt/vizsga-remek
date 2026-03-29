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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<Felhasznalo> _hasher;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<Felhasznalo>();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var user = await _context.Felhasznalok
                .FirstOrDefaultAsync(x => x.FelhasznaloNev == model.Username);

            if (user == null)
                return Unauthorized();

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FelhasznaloNev ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.FelhasznaloID.ToString()),
                new Claim("isAdmin", user.IsAdmin.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(6),
                signingCredentials: creds
            );

            return Ok(new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(CreateFelhasznaloDto dto)
        {
            if (await _context.Felhasznalok.AnyAsync(x => x.FelhasznaloNev == dto.FelhasznaloNev))
                return BadRequest("Felhasználónév foglalt");

            var user = new Felhasznalo
            {
                FelhasznaloNev = dto.FelhasznaloNev,
                TeljesNev = dto.TeljesNev,
                Email = dto.Email,
                SzuletesiDatum = dto.SzuletesiDatum,
                RegDatum = DateTime.UtcNow,
                IsAdmin = false
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _context.Felhasznalok.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("me")]
        public async Task<ActionResult<FelhasznaloDTO>> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

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
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateFelhasznaloDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Felhasznalok.FirstOrDefaultAsync(x => x.FelhasznaloID == userId);

            if (user == null)
                return NotFound();

            var usernameExists = await _context.Felhasznalok.AnyAsync(x =>
                x.FelhasznaloNev == dto.FelhasznaloNev &&
                x.FelhasznaloID != userId);

            if (usernameExists)
                return BadRequest("Ez a felhasználónév már foglalt.");

            user.FelhasznaloNev = dto.FelhasznaloNev;
            user.TeljesNev = dto.TeljesNev;
            user.Email = dto.Email;
            user.SzuletesiDatum = dto.SzuletesiDatum;
            user.Bio = dto.Bio;
            user.Helyszin = dto.Helyszin;
            user.Klub = dto.Klub;

            if (dto.AvatarUrl == null)
                user.AvatarUrl = null;
            else if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
                user.AvatarUrl = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{dto.AvatarUrl}";

            if (dto.CoverUrl == null)
                user.CoverUrl = null;
            else if (!string.IsNullOrWhiteSpace(dto.CoverUrl))
                user.CoverUrl = $"{Request.Scheme}://{Request.Host}/uploads/covers/{dto.CoverUrl}";

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file");

            var folder = Path.Combine("wwwroot", "uploads", "avatars");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new { imageUrl = fileName });
        }

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