using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using Client.Models;

namespace Client.Services;
public class EventParticipantApi(HttpClient http)
{
    public async Task<List<EventParticipant>> GetActiveEventParticipantsAsync(CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/eventparticipants");
            var response = await http.GetAsync("/api/eventparticipants", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                var result = await response.Content.ReadFromJsonAsync<List<EventParticipant>>(cancellationToken: ct) ?? [];
                Console.WriteLine($"API call successful, received {result.Count} event participants");
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

    public async Task<int?> GetUserByMemberIdAsync(int memberId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/eventparticipants/member/{memberId}");
            var response = await http.GetAsync($"/api/eventparticipants/member/{memberId}", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                var result = await response.Content.ReadFromJsonAsync<int?>(cancellationToken: ct);
                Console.WriteLine($"API call successful, received user ID: {result}");
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

    public async Task<List<int>> GetMemberIdsByUserIdAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/eventparticipants/user/{userId}");
            var response = await http.GetAsync($"/api/eventparticipants/user/{userId}", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                var result = await response.Content.ReadFromJsonAsync<List<int>>(cancellationToken: ct) ?? [];
                Console.WriteLine($"API call successful, received {result.Count} member IDs");
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

    public async Task<EventParticipant?> GetEventParticipantByIdAsync(int eventId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/eventparticipants/{eventId}");
            var response = await http.GetAsync($"/api/eventparticipants/{eventId}", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                var result = await response.Content.ReadFromJsonAsync<EventParticipant>(cancellationToken: ct);
                Console.WriteLine($"API call successful, received event participant: {result}");
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

    public async Task<int> GetMemberIdByUserIdAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/eventparticipants/member/user/{userId}");
            var response = await http.GetAsync($"/api/eventparticipants/member/user/{userId}", ct);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                var result = await response.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
                Console.WriteLine($"API call successful, received member ID: {result}");
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

    public async Task<EventParticipant?> RegisterForEventAsync(object registrationRequest, CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"Making API call to: {http.BaseAddress}api/eventparticipants/register");
            var response = await http.PostAsJsonAsync("/api/eventparticipants/register", registrationRequest, ct);
            Console.WriteLine($"Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {content}");

                var result = await response.Content.ReadFromJsonAsync<EventParticipant>(cancellationToken: ct);
                Console.WriteLine($"API call successful, registered event participant: {result}");
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

}


