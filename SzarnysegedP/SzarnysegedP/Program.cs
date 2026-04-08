using Blazored.LocalStorage;
using SzarnysegedP.Components;
using SzarnysegedP.Services;

// ===== ALKALMAZÁS ÉPÍTÉS =====
// A WebApplication builder létrehozása – ugyanaz a minta, mint az API-nál
var builder = WebApplication.CreateBuilder(args);

// ===== BLAZOR SZOLGÁLTATÁSOK =====
// Razor komponensek regisztrálása (a .razor fájlok mûködéséhez kell)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();  // Blazor Server mód: a logika a SZERVEREN fut,
                                        // a böngészõ és a szerver SignalR websocket-en kommunikál
                                        // (nem WebAssembly, ahol a kód a böngészõben futna)

// ===== HTTP KLIENSEK KONFIGURÁLÁSA =====
// Typed HttpClient a HirService-hez – dedikált, elõre konfigurált HttpClient
// Az AddHttpClient<T> a DI-ban automatikusan létrehozza és injektálja a HirService-be
builder.Services.AddHttpClient<HirService>(client =>
{
    // Az API alap URL-je az appsettings.json "ApiBaseUrl" mezõjébõl
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

// Általános HttpClient regisztrálása – a többi szolgáltatás (AuthService, AdminService) ezt használja
// AddScoped: minden felhasználói kéréshez (scope) külön példány jön létre
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!) // Ugyanaz az API alap URL
    };
    return client;
});

// ===== SAJÁT SZOLGÁLTATÁSOK REGISZTRÁLÁSA =====
// Scoped: minden felhasználói kapcsolathoz (SignalR circuit) külön példány
builder.Services.AddScoped<AuthService>();      // Bejelentkezés, regisztráció, token kezelés
builder.Services.AddScoped<AdminService>();     // Admin mûveletek (felhasználók, spotok kezelése)

// ===== KÜLSÕ KÖNYVTÁRAK =====
// Blazored.LocalStorage: lehetõvé teszi a böngészõ localStorage elérését C#-ból
// Az AuthService itt tárolja a JWT tokent bejelentkezés után
builder.Services.AddBlazoredLocalStorage();

// Authorizációs szolgáltatás a kliens oldalon
// Ez teszi lehetõvé az [Authorize] attribútum és AuthorizeView komponens használatát a Razor oldalakban
builder.Services.AddAuthorizationCore();

// ===== ALKALMAZÁS FELÉPÍTÉSE =====
var app = builder.Build();

// Éles (nem fejlesztõi) környezetben hibakezelés és HSTS bekapcsolása
if (!app.Environment.IsDevelopment())
{
    // Központi hibakezelõ oldal – nem kezelt kivételeknél ide irányít
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS (HTTP Strict Transport Security): a böngészõt HTTPS használatára kényszeríti
    app.UseHsts();
}

app.UseHttpsRedirection();  // HTTPS átirányítás – HTTP kéréseket átirányítja HTTPS-re
app.UseStaticFiles();       // Statikus fájlok kiszolgálása a wwwroot mappából (CSS, JS, képek, favicon)
app.UseAntiforgery();       // Antiforgery (CSRF védelem) – megakadályozza a cross-site request forgery támadásokat
                            // A Blazor Server automatikusan használja az ûrlapoknál

// ===== BLAZOR ROUTING KONFIGURÁLÁSA =====
// Az App komponens lesz a gyökér komponens (minden oldal ezen belül renderelõdik)
// Az AddInteractiveServerRenderMode() engedélyezi az interaktív szerver oldali renderelést
// (SignalR-en keresztül valós idejû UI frissítés)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();