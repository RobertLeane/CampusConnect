using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public class ClubApi(HttpClient http)
{
    public async Task<List<Club>> GetClubsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<Club>>("/api/clubs", ct) ?? [];

    public async Task<Club?> GetClubByIdAsync(int clubId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<Club>($"/api/clubs/{clubId}", ct);
}
