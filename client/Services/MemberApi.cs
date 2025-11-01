using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public class MemberApi(HttpClient http)
{
    private readonly HttpClient _http = http;
    // Implementations for calling Member-related API endpoints

    public async Task<List<Member>> GetClubMembersAsync(int clubId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {_http.BaseAddress}api/members/clubs/{clubId}/members");
            var response = await _http.GetAsync($"/api/members/clubs/{clubId}/members", ct).ConfigureAwait(false);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                Console.WriteLine($"Response content: {content}");
                var result = await response.Content.ReadFromJsonAsync<List<Member>>(cancellationToken: ct).ConfigureAwait(false) ?? new List<Member>();
                Console.WriteLine($"API call successful, received {result.Count} members");
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            Console.WriteLine($"API call failed with status {response.StatusCode}: {errorContent}");
            throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call exception: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Member>> GetUserMembershipsAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {_http.BaseAddress}api/users/{userId}/memberships");
            var response = await _http.GetAsync($"/api/members/users/{userId}/memberships", ct).ConfigureAwait(false);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<Member>>(cancellationToken: ct).ConfigureAwait(false) ?? new List<Member>();
                Console.WriteLine($"API call successful, received {result.Count} memberships");
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            Console.WriteLine($"API call failed with status {response.StatusCode}: {errorContent}");
            throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call exception: {ex.Message}");
            throw;
        }
    }

    public async Task<Member?> AddMemberToClubAsync(int userId, int clubId, CancellationToken ct = default)
    {
        try
        {
            var payload = new { UserId = userId, Role = "Member" };
            Console.WriteLine($"Posting to: {_http.BaseAddress}api/members/clubs/{clubId}/members with payload: {{ UserId: {userId}, Role: \"Member\" }}");
            var response = await _http.PostAsJsonAsync($"/api/members/clubs/{clubId}/members", payload, ct).ConfigureAwait(false);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<Member>(cancellationToken: ct).ConfigureAwait(false);
                Console.WriteLine($"Created member with id: {created?.MemberId}");
                return created;
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            Console.WriteLine($"API call failed with status {response.StatusCode}: {errorContent}");
            throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call exception: {ex.Message}");
            throw;
        }
    }

}

