// TOKEN RESPONSE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    // A bejelentkezés válasza – az API ezt küldi vissza sikeres login után
    // Az AuthController.Login() hozza létre: return Ok(new TokenResponse { Token = ... });
    // A kliens (AuthService.Login()) fogadja és elmenti a localStorage-ba
    public class TokenResponse
    {
        public string Token { get; set; }   // A JWT token Base64 string formátumban
                                            // A kliens ezt tárolja és minden API kérésnél elküldi
                                            // az Authorization headerben: "Bearer eyJhb..."
    }
}
