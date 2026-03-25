using System.Net.Http.Json;
using SzarnysegedShared.DTOs.HirDTOs;

public class HirService
{
    private readonly HttpClient _http;

    public HirService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<HirDto>> GetHirekAsync()
    {
        return await _http.GetFromJsonAsync<List<HirDto>>("api/hirek")
               ?? new List<HirDto>();
    }
}