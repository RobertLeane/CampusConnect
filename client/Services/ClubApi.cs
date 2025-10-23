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
            var response = await http.GetAsync("/api/clubs", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");
                
                var result = await response.Content.ReadFromJsonAsync<List<Club>>(cancellationToken: ct) ?? [];
                Console.WriteLine($"API call successful, received {result.Count} clubs");
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API call failed with status {response.StatusCode}: {errorContent}");
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call exception: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<Club?> GetClubByIdAsync(int clubId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<Club>($"/api/clubs/{clubId}", ct);

    public async Task<List<Club>> GetUserJoinedClubsAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/clubs/user/{userId}");
            var response = await http.GetAsync($"/api/clubs/user/{userId}", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");
                
                var result = await response.Content.ReadFromJsonAsync<List<Club>>(cancellationToken: ct) ?? [];
                Console.WriteLine($"API call successful, received {result.Count} joined clubs");
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API call failed with status {response.StatusCode}: {errorContent}");
                return new List<Club>(); // Return empty list on error
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call exception: {ex.Message}");
            return new List<Club>(); // Return empty list on exception
        }
    }
}