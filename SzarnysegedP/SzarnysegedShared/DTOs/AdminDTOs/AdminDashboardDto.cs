// ADMIN DTO-K
namespace SzarnysegedShared.DTOs.AdminDTOs
{
    // ===== ADMIN DASHBOARD DTO =====
    // Az admin felület főoldalán megjelenő összesített statisztikákat tartalmazza
    // Az AdminController.GetDashboard() állítja össze és küldi vissza
    // Az Admin.razor fogadja és jeleníti meg (statisztika kártyák + Chart.js diagram)
    public class AdminDashboardDto
    {
        // Összesített számok – az 5 statisztika kártya adatai
        public int UsersCount { get; set; }                 // Összes regisztrált felhasználó
        public int PostsCount { get; set; }                 // Összes fórum bejegyzés
        public int NewsCount { get; set; }                  // Összes hír
        public int SpotsCount { get; set; }                 // Összes spot (repülős helyszín)
        public int SpotSuggestionsCount { get; set; }       // Feldolgozatlan spot javaslatok száma

        // Napi bontású statisztika az utolsó 7 napra – a Chart.js diagram adatforrása
        // A lista elemeit az AdminController for ciklussal tölti fel (start → today)
        // "= new()" → üres listával inicializálva (NullReferenceException megelőzése)
        public List<AdminDailyStatDto> Last7Days { get; set; } = new(); 
    }

    // ===== NAPI STATISZTIKA DTO =====
    // A Last7Days lista egy eleme – egyetlen nap adatai
    // A Chart.js vonaldiagram minden adatsora (Users, Posts, News, Suggestions)
    // ennek a listának a megfelelő mezőjéből épül fel
    public class AdminDailyStatDto
    {
        public string Label { get; set; } = string.Empty;   // Az X tengely címkéje (pl. "04.07")
        public int Users { get; set; }                      // Aznap regisztrált felhasználók száma
        public int Posts { get; set; }                      // Aznap írt bejegyzések száma
        public int News { get; set; }                       // Aznap publikált hírek száma
        public int Spots { get; set; }                      // Aznap létrehozott spotok száma (jelenleg fix 0)
        public int Suggestions { get; set; }                // Aznap beküldött javaslatok száma
    }
}