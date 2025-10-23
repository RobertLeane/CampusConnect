using System.Net.Http.Json;
using Client.Models;
using System.Text;

namespace Client.Services;

public class EventApi
{
    private readonly HttpClient _httpClient;

    // Constructor to inject HttpClient
    public EventApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Method to get events
    public async Task<List<Event>> GetEventsAsync()
    {
        var response = await _httpClient.GetAsync("api/events");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Event>>() ?? new List<Event>();
        }
        throw new Exception("Error retrieving events");
    }

    // Method to get user's RSVP'd events
    public async Task<List<Event>> GetUserRsvpEventsAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/events/user/{userId}/rsvps");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Event>>() ?? new List<Event>();
        }
        return new List<Event>(); // Return empty list if error or no events
    }

    // Method to RSVP for an event
    public async Task<HttpResponseMessage> RsvpAsync(int eventId, int userId)
    {
        // Send the POST request with userId as query parameter to match server expectation
        var response = await _httpClient.PostAsync($"api/events/{eventId}/rsvp?userId={userId}", null);
        return response;
    }
}
