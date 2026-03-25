using System.Net.Http.Headers;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;
using Blazored.LocalStorage;

namespace SzarnysegedP.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

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

            await _localStorage.SetItemAsync("token", result.Token);

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Token);

            return true;
        }
        public async Task<bool> Register(CreateFelhasznaloDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", dto);
            var text = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"STATUS: {response.StatusCode}");
            Console.WriteLine($"RESPONSE: {text}");

            return response.IsSuccessStatusCode;
        }
    }
}
