using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;


namespace SzarnysegedP.Services
{
    // ===== AUTENTIKÁCIÓS SZOLGÁLTATÁS =====
    public class AuthService
    {
        // HttpClient: az API-val való kommunikációhoz
        private readonly HttpClient _http;
        // ILocalStorageService: a böngésző localStorage-ába ír/olvas
        // A JWT tokent itt tároljuk, így oldal újratöltés után is megmarad
        private readonly ILocalStorageService _localStorage;

        // ===== ESEMÉNY (EVENT) =====
        public event Action? AuthStateChanged;

        // Konstruktor – DI-ból kapjuk a HttpClient-et és a localStorage szolgáltatást
        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        // ===== BEJELENTKEZÉS =====
        // A LoginDTO-t elküldi az API-nak, és ha sikeres, eltárolja a JWT tokent
        public async Task<bool> Login(LoginDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", dto);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.Token))
                return false;

            await _localStorage.SetItemAsync("token", result.Token);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Token);

            AuthStateChanged?.Invoke();
            return true;
        }

        // ===== REGISZTRÁCIÓ =====
        public async Task<bool> Register(CreateFelhasznaloDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", dto);
            return response.IsSuccessStatusCode;
        }

        // ===== KIJELENTKEZÉS =====
        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
            _http.DefaultRequestHeaders.Authorization = null;
            AuthStateChanged?.Invoke();
        }

        // ===== TOKEN LEKÉRÉSE =====
        public async Task<string?> GetToken()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>("token");
            }
            catch
            {
                return null;
            }
        }

        // ===== BEJELENTKEZVE VAN-E =====
        public async Task<bool> IsLoggedIn()
        {
            var token = await GetToken();
            return !string.IsNullOrWhiteSpace(token);
        }

        // ===== FELHASZNÁLÓNÉV KIOLVASÁSA A TOKENBŐL =====
        public async Task<string?> GetUsernameFromToken()
        {
            var token = await GetToken();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                return null;

            var jwt = handler.ReadJwtToken(token);

            return jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Name ||
                c.Type == "unique_name" ||
                c.Type == "name")?.Value;
        }

        // ===== FELHASZNÁLÓ ID KIOLVASÁSA A TOKENBŐL =====
        public async Task<int?> GetUserIdFromToken()
        {
            var token = await GetToken();

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                return null;

            var jwt = handler.ReadJwtToken(token);

            var idValue = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "nameid")?.Value;

            if (int.TryParse(idValue, out int userId))
                return userId;

            return null;
        }

        // ===== AUTHORIZATION HEADER BEÁLLÍTÁSA =====
        public async Task SetAuthorizationHeader()
        {
            var token = await GetToken();

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ===== BEJELENTKEZETT FELHASZNÁLÓ ADATAI =====
        public async Task<FelhasznaloDTO?> GetCurrentUser()
        {
            await SetAuthorizationHeader();

            var response = await _http.GetAsync("api/auth/me");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<FelhasznaloDTO>();
        }

        // ===== PROFIL FRISSÍTÉSE =====
        public async Task<bool> UpdateProfile(UpdateFelhasznaloDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _http.PutAsJsonAsync("api/auth/profile", dto);
            return response.IsSuccessStatusCode;
        }

        // ===== AVATAR (PROFILKÉP) FELTÖLTÉSE =====
        public async Task<string?> UploadAvatar(Stream fileStream, string fileName)
        {
            await SetAuthorizationHeader();

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("api/auth/upload-avatar", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
            return result?.ImageUrl;
        }

        // ===== BORÍTÓKÉP FELTÖLTÉSE =====
        public async Task<string?> UploadCover(Stream fileStream, string fileName)
        {
            await SetAuthorizationHeader();

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("api/auth/upload-cover", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
            return result?.ImageUrl;
        }

        // ===== FÓRUM KÉP FELTÖLTÉSE =====
        public async Task<string?> UploadForumImage(Stream fileStream, string fileName)
        {
            await SetAuthorizationHeader();

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("api/forum/upload-image", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<ImageUploadResponse>();
            return result?.ImageUrl;
        }

        // ===== ADMIN JOGOSULTSÁG ELLENŐRZÉSE =====
        public async Task<bool> IsAdmin()
        {
            try
            {
                var token = await GetToken();

                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(token))
                    return false;

                var jwt = handler.ReadJwtToken(token);

                var adminValue = jwt.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value;

                return bool.TryParse(adminValue, out var isAdmin) && isAdmin;
            }
            catch
            {
                return false;
            }
        }

        // ===== BELSŐ SEGÉDOSZTÁLY =====
        public class ImageUploadResponse
        {
            public string? ImageUrl { get; set; }
        }
    }
}