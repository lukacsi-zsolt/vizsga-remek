// CREATE FELHASZNÁLÓ DTO
using System.ComponentModel.DataAnnotations;

namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    // A regisztrációs kérés adatai
    // A kliens (Register.razor) ezt küldi az API-nak: POST api/auth/register
    // Az AuthController.Register() metódus fogadja
    //
    // A [Required] attribútum (Data Annotation) kétféle validációt biztosít:
    //   1. SZERVER OLDALON: az [ApiController] attribútum automatikusan ellenőrzi
    //      → ha hiányzik egy [Required] mező, 400 Bad Request választ küld
    //   2. KLIENS OLDALON: a Blazor EditForm komponens is felhasználhatja validációhoz
    public class CreateFelhasznaloDto
    {
        [Required]                                      // Kötelező mező – nem lehet null/üres
        public string? FelhasznaloNev { get; set; }     // Kívánt felhasználónév

        [Required]
        public string? TeljesNev { get; set; }          // Teljes név (a Register.razor összefűzi: lastName + firstName)
        [Required]
        public string? Email { get; set; }              // E-mail cím
        [Required]
        public DateTime? SzuletesiDatum { get; set; }   // Születési dátum
        [Required]
        public string Password { get; set; }            // Nyers jelszó – az API hasheli el (PasswordHasher)
                                                        // FONTOS: string (nem nullable!) mert a jelszó kötelező
                                                        // A [Required] + nem-nullable string kettős védelem
    }
    // MEGJEGYZÉS: nincs IsAdmin mező!
    // Az AuthController.Register() mindig false-ra állítja → senki nem regisztrálhat adminként
    // Ez biztonsági szempont: a kliens nem manipulálhatja a jogosultságot
}