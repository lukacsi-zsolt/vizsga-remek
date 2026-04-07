using ErnyosKozoApi.Data;                            // Az AppDbContext osztályt tartalmazza (adatbázis kontextus)
using Microsoft.EntityFrameworkCore;                 // Entity Framework Core – ORM az adatbázis-kezeléshez
using Microsoft.EntityFrameworkCore.SqlServer;       // SQL Server adatbázis-provider az EF Core-hoz
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT (JSON Web Token) alapú autentikáció
using Microsoft.IdentityModel.Tokens;                // Token validálási paraméterek beállításához
using System.Text;                                   // Encoding osztály használatához (UTF8 kódolás)

// WebApplication builder létrehozása – ez gyûjti össze a szolgáltatásokat (DI container) és a konfigurációt
var builder = WebApplication.CreateBuilder(args);

// ===== JWT KONFIGURÁCIÓ =====
// A titkos kulcs kiolvasása az appsettings.json "Jwt:Key" mezõjébõl, majd byte tömbbé alakítása
// Ez a kulcs fogja aláírni és ellenõrizni a tokeneket
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

// ===== AUTENTIKÁCIÓ BEÁLLÍTÁSA =====
// Az autentikációs szolgáltatás hozzáadása a DI containerhez
builder.Services.AddAuthentication(options =>
{
    // Az alapértelmezett autentikációs séma: JWT Bearer token
    // Ez határozza meg, hogy a rendszer hogyan azonosítja a bejelentkezett felhasználót
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // Ha a felhasználó nem autentikált, ezzel a sémával "kihívás" (challenge) küldése (401-es válasz)
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Token validálási szabályok meghatározása
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,             // Nem ellenõrizzük a token kibocsátóját (ki adta ki)
        ValidateAudience = false,           // Nem ellenõrizzük a token célközönségét (kinek szól)
        ValidateLifetime = true,            // Ellenõrizzük, hogy a token nem járt-e le (exp claim)
        ValidateIssuerSigningKey = true,    // Ellenõrizzük, hogy a token aláírása érvényes-e
        // A szimmetrikus kulcs, amellyel a tokenek aláírását ellenõrizzük
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Authorizáció (jogosultságkezelés) szolgáltatás regisztrálása – [Authorize] attribútum használatához kell
builder.Services.AddAuthorization();

// ===== ADATBÁZIS KONFIGURÁCIÓ =====
// Az AppDbContext regisztrálása a DI containerbe
// Az SQL Server-t használjuk, a connection stringet az appsettings.json "DefaultConnection" mezõjébõl olvassuk
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== EGYÉB SZOLGÁLTATÁSOK =====
// Controller-ek regisztrálása (az API végpontokat tartalmazó osztályok)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== CORS (Cross-Origin Resource Sharing) =====
// Lehetõvé teszi, hogy más domainrõl (pl. frontend) is hívhassák az API-t
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowAll",
        policy => policy
            .AllowAnyOrigin()   // Bármilyen domainrõl jöhet kérés
            .AllowAnyHeader()   // Bármilyen HTTP fejléccel
            .AllowAnyMethod()); // Bármilyen HTTP metódussal (GET, POST, PUT, DELETE stb.)
});

// ===== ALKALMAZÁS FELÉPÍTÉSE =====
// A builder.Build() létrehozza magát a WebApplication-t a fent regisztrált szolgáltatásokkal
var app = builder.Build();
// CORS middleware aktiválása az "allowAll" szabállyal
app.UseCors("allowAll");
// Statikus fájlok kiszolgálása a wwwroot mappából (pl. képek, CSS, JS)
app.UseStaticFiles();

// Fejlesztõi környezetben a Swagger UI engedélyezése az API teszteléséhez
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Swagger JSON végpont
    app.UseSwaggerUI(); // Swagger felhasználói felület (böngészõben elérhetõ)
}

// HTTPS átirányítás – HTTP kéréseket átirányítja HTTPS-re
app.UseHttpsRedirection();

// ===== MIDDLEWARE SORREND (FONTOS!) =====
// Az Authentication MINDIG az Authorization ELÕTT kell legyen!
// Elõször azonosítjuk a felhasználót (ki vagy?) ...
app.UseAuthentication();
// ... majd ellenõrizzük a jogosultságait (mit csinálhatsz?)
app.UseAuthorization();
// A controller végpontok hozzárendelése az útvonalakhoz (route-okhoz)
app.MapControllers();
// Az alkalmazás elindítása – ettõl kezdve fogadja a HTTP kéréseket
app.Run();