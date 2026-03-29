using System.Net.Http.Json;
using SzarnysegedShared.DTOs.AdminDTOs;
using SzarnysegedShared.DTOs.ForumDTOs;
using SzarnysegedShared.DTOs.HirDTOs;

namespace SzarnysegedP.Services
{
    public class AdminService
    {
        private readonly HttpClient _http;
        private readonly AuthService _auth;

        public AdminService(HttpClient http, AuthService auth)
        {
            _http = http;
            _auth = auth;
        }

        private async Task Prepare()
        {
            await _auth.SetAuthorizationHeader();
        }

        public async Task<AdminDashboardDto?> GetDashboard()
        {
            await Prepare();
            return await _http.GetFromJsonAsync<AdminDashboardDto>("api/admin/dashboard");
        }

        public async Task<List<AdminUserDto>> GetUsers()
        {
            await Prepare();
            return await _http.GetFromJsonAsync<List<AdminUserDto>>("api/admin/users") ?? new();
        }

        public async Task<bool> UpdateUser(AdminUserDto dto)
        {
            await Prepare();
            var response = await _http.PutAsJsonAsync($"api/admin/users/{dto.FelhasznaloID}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveAvatar(int userId)
        {
            await Prepare();
            var response = await _http.PostAsync($"api/admin/users/{userId}/remove-avatar", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveCover(int userId)
        {
            await Prepare();
            var response = await _http.PostAsync($"api/admin/users/{userId}/remove-cover", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            await Prepare();
            var response = await _http.DeleteAsync($"api/admin/users/{userId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<HirDto>> GetNews()
        {
            await Prepare();
            return await _http.GetFromJsonAsync<List<HirDto>>("api/admin/news") ?? new();
        }

        public async Task<bool> CreateNews(CreateHirDto dto)
        {
            await Prepare();
            var response = await _http.PostAsJsonAsync("api/hirek", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateNews(int id, UpdateHirDto dto)
        {
            await Prepare();
            var response = await _http.PutAsJsonAsync($"api/hirek/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteNews(int id)
        {
            await Prepare();
            var response = await _http.DeleteAsync($"api/hirek/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Spot>> GetSpots()
        {
            await Prepare();
            return await _http.GetFromJsonAsync<List<Spot>>("api/admin/spots") ?? new();
        }

        public async Task<bool> UpdateSpot(Spot dto)
        {
            await Prepare();
            var response = await _http.PutAsJsonAsync($"api/spotok/{dto.SpotID}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSpot(int id)
        {
            await Prepare();
            var response = await _http.DeleteAsync($"api/spotok/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<SpotJavaslat>> GetSpotSuggestions()
        {
            await Prepare();
            return await _http.GetFromJsonAsync<List<SpotJavaslat>>("api/admin/spot-suggestions") ?? new();
        }

        public async Task<bool> ApproveSpotSuggestion(int id)
        {
            await Prepare();
            var response = await _http.PostAsync($"api/admin/spot-suggestions/{id}/approve", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSpotSuggestion(int id)
        {
            await Prepare();
            var response = await _http.DeleteAsync($"api/admin/spot-suggestions/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<BejegyzesListaDto>> GetPosts()
        {
            await Prepare();
            return await _http.GetFromJsonAsync<List<BejegyzesListaDto>>("api/admin/posts") ?? new();
        }

        public async Task<bool> DeletePost(int id)
        {
            await Prepare();
            var response = await _http.DeleteAsync($"api/forum/bejegyzesek/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteComment(int id)
        {
            await Prepare();
            var response = await _http.DeleteAsync($"api/forum/kommentek/{id}");
            return response.IsSuccessStatusCode;
        }
    }

    public class Spot
    {
        public int SpotID { get; set; }
        public string? Nev { get; set; }
        public string? Slug { get; set; }
        public string? Orszag { get; set; }
        public string? Megye { get; set; }
        public string? HelyLeiras { get; set; }
        public int? Magassag { get; set; }
        public double? AtlagSzel { get; set; }
        public string? Szabalyok { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public int? LetrehozoFelhasznaloID { get; set; }
    }

    public class SpotJavaslat
    {
        public int SpotJavaslatID { get; set; }
        public string? Nev { get; set; }
        public string? Orszag { get; set; }
        public string? Megye { get; set; }
        public string? HelyLeiras { get; set; }
        public int? Magassag { get; set; }
        public double? AtlagSzel { get; set; }
        public string? Szabalyok { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public int? BekuldoFelhasznaloID { get; set; }
        public DateTime Letrehozva { get; set; }
        public bool Feldolgozva { get; set; }
        public string? AdminMegjegyzes { get; set; }
    }
}