using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;


public class ClubApi(HttpClient http)
{
    public async Task<List<Club>> GetClubsAsync(CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/clubs");
            var result = await http.GetFromJsonAsync<List<Club>>("/api/clubs", ct) ?? [];
            Console.WriteLine($"API call successful, received {result.Count} clubs");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
            throw;
        }
    }

    public async Task<Club?> GetClubByIdAsync(int clubId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<Club>($"/api/clubs/{clubId}", ct);
}