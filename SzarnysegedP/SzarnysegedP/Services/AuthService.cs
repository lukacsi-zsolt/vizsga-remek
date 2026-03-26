using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;

namespace SzarnysegedP.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public event Action? AuthStateChanged;

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

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

        public async Task<bool> Register(CreateFelhasznaloDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
            _http.DefaultRequestHeaders.Authorization = null;
            AuthStateChanged?.Invoke();
        }

        public async Task<string?> GetToken()
        {
            return await _localStorage.GetItemAsync<string>("token");
        }

        public async Task<bool> IsLoggedIn()
        {
            var token = await GetToken();
            return !string.IsNullOrWhiteSpace(token);
        }

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

        public async Task SetAuthorizationHeader()
        {
            var token = await GetToken();

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<FelhasznaloDTO?> GetCurrentUser()
        {
            await SetAuthorizationHeader();

            var response = await _http.GetAsync("api/auth/me");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<FelhasznaloDTO>();
        }

        public async Task<bool> UpdateProfile(UpdateFelhasznaloDto dto)
        {
            await SetAuthorizationHeader();

            var response = await _http.PutAsJsonAsync("api/auth/profile", dto);
            return response.IsSuccessStatusCode;
        }
    }
}