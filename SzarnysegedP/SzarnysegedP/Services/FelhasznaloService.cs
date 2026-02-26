using System.Net.Http.Json;
using SzarnysegedShared.DTOs.FelhasznaloDTOs;

public class FelhasznaloService
{
    private readonly HttpClient _http;

    public FelhasznaloService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ApiClient");
    }

    public async Task<List<FelhasznaloDTO>> GetHirekAsync()
    {
        return await _http.GetFromJsonAsync<List<FelhasznaloDTO>>("api/hirek")
               ?? new List<FelhasznaloDTO>();
    }
}