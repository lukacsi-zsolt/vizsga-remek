using ErnyosKozoApi.Data;
using ErnyosKozoApi.Models;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            //token csinalas innentol
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FelhasznaloNev),
                new Claim(ClaimTypes.NameIdentifier, user.FelhasznaloID.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );
            return Ok(new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (await _context.Felhasznalok.AnyAsync(x => x.FelhasznaloNev == dto.Username))
                return BadRequest("Felhasználónév foglalt");

            var user = new Felhasznalo
            {
                FelhasznaloNev = dto.Username,
                TeljesNev = dto.TeljesNev,
                Email = dto.Email,
                SzuletesiDatum = dto.SzuletesiDatum
            };

            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _context.Felhasznalok.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
    
    }
}
