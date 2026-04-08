using System.Net.Http.Json;
using SzarnysegedShared.DTOs.HirDTOs;

// ===== HÍR SZOLGÁLTATÁS =====
public class HirService
{
    private readonly HttpClient _http;

    // Konstruktor – a DI automatikusan injektálja a Typed HttpClient-et
    public HirService(HttpClient http)
    {
        _http = http;
    }

    // ===== ÖSSZES HÍR LEKÉRDEZÉSE =====
    public async Task<List<HirDto>> GetHirekAsync()
    {
        return await _http.GetFromJsonAsync<List<HirDto>>("api/hirek")
               ?? new List<HirDto>();
    }

    // ===== EGY HÍR LEKÉRDEZÉSE ID ALAPJÁN =====
    public async Task<HirDto?> GetHirByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<HirDto>($"api/hirek/{id}");
    }
}