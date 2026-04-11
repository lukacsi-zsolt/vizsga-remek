// FELHASZNÁLÓ DTO
namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    // A felhasználó PUBLIKUS adatai – ezt kapja a kliens profil lekérdezéskor
    // Használja: AuthController.Me(), FelhasznalokController.GetByUsername()
    // ÖSSZEHASONLÍTÁS A FELHASZNALO ENTITÁSSAL:
    // Felhasznalo (entitás):     FelhasznaloDTO (DTO):
    //   FelhasznaloID      →       FelhasznaloID        ✓ (átkerül)
    //   FelhasznaloNev     →       FelhasznaloNev       ✓ (átkerül)
    //   PasswordHash       →       ---                  ✗ (NEM KERÜL ÁT! – biztonsági ok)
    //   RegDatum           →       ---                  ✗ (nem szükséges a kliensen)
    //   IsAdmin            →       IsAdmin              ✓ (átkerül)
    public class FelhasznaloDTO
    {
        public int FelhasznaloID { get; set; }
        public string? FelhasznaloNev { get; set; }
        public string? TeljesNev { get; set; }
        public string? Email { get; set; }
        public DateTime? SzuletesiDatum { get; set; }
        public string? Bio { get; set; }
        public string? Helyszin { get; set; }
        public string? Klub { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public bool IsAdmin { get; set; }
        // A PasswordHash SZÁNDÉKOSAN nincs itt!
        // Ez a DTO lényege: csak a biztonságos, megjelenítendő adatok kerülnek át
    }
}