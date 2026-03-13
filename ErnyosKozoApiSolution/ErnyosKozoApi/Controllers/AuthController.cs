using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            private readonly IConfiguration _config;

            public AuthController(IConfiguration config)
            {
                _config = config;
            }

            [HttpPost("login")]
            public IActionResult Login(LoginDto model)
            {
                // Validate user from database
                if (model.Username != "admin" || model.Password != "123")
                    return Unauthorized();

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, model.Username)
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("secret key"));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
        }
}
