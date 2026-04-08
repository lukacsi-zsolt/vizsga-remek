using System.Net.Http.Json;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;

// ===== FELHASZNÁLÓ SZOLGÁLTATÁS =====
public class FelhasznaloService
{
    private readonly HttpClient _http;

    // ===== KONSTRUKTOR – IHttpClientFactory MINTA =====
    public FelhasznaloService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ApiClient");
    }

    // ===== ÖSSZES FELHASZNÁLÓ LEKÉRDEZÉSE =====
    public async Task<List<FelhasznaloDTO>> GetFelhasznalokAsync()
    {
        return await _http.GetFromJsonAsync<List<FelhasznaloDTO>>("api/felhasznalok")
               ?? new List<FelhasznaloDTO>();
    }
}