using System.Security.Claims;   // ClaimsPrincipal, ClaimTypes, Claim osztályok

namespace ErnyosKozoApi.Helpers
{
    // ===== KITERJESZTŐ METÓDUSOK (EXTENSION METHODS) =====
    // A "static class" kötelező a kiterjesztő metódusokhoz
    // A kiterjesztő metódusok lehetővé teszik, hogy egy meglévő osztályhoz (jelen esetben ClaimsPrincipal)
    // új metódusokat adjunk anélkül, hogy módosítanánk az eredeti osztályt
    // Használat a controllerekben: User.IsAdmin(), User.GetUserId()
    public static class ClaimsPrincipalExtensions
    {
        // ===== ADMIN ELLENŐRZÉS =====
        // Megvizsgálja, hogy a bejelentkezett felhasználó admin-e
        // A "this ClaimsPrincipal user" teszi kiterjesztő metódussá:
        //   - a "this" kulcsszó jelzi, hogy ez egy extension method
        //   - a ClaimsPrincipal az a típus, amelyet "kiterjesztünk"
        //   - így hívható: User.IsAdmin() (mintha a ClaimsPrincipal saját metódusa lenne)
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            // A JWT tokenben az AuthController.Login()-ban beállított "isAdmin" claim keresése
            // A FindFirst visszaadja az első ilyen nevű claim-et, vagy null-t ha nincs
            // A "?." (null-conditional operator) megvéd a NullReferenceException-tól
            var claim = user.FindFirst("isAdmin")?.Value;
            // bool.TryParse: a string-et ("True"/"False") bool-lá konvertálja
            // Két feltétel ÉS kapcsolatban:
            //   1. A TryParse sikerült (érvényes bool string volt)
            //   2. Az eredmény true (tényleg admin)
            // Ha a claim null vagy nem "True" → false-t ad vissza
            return bool.TryParse(claim, out var isAdmin) && isAdmin;
        }

        // ===== FELHASZNÁLÓ ID KIOLVASÁSA =====
        // Visszaadja a bejelentkezett felhasználó adatbázis-beli ID-ját a JWT tokenből
        // Visszatérési típus: int? (nullable int) – null ha nincs bejelentkezett felhasználó
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            // A ClaimTypes.NameIdentifier egy szabványos claim típus ("nameid")
            // Az AuthController.Login()-ban ezt a felhasználó ID-jával töltöttük fel
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // int.TryParse: string → int konverzió biztonságosan (nem dob kivételt ha hibás)
            // Ha sikeres → visszaadjuk az ID-t
            if (int.TryParse(value, out var id))
                return id;

            // Ha a claim hiányzik vagy nem szám → null (nincs érvényes felhasználó)
            return null;
        }
    }
}