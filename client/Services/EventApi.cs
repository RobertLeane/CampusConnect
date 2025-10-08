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
            return await response.Content.ReadFromJsonAsync<List<Event>>();
        }
        throw new Exception("Error retrieving events");
    }

    // Method to RSVP for an event
    public async Task<HttpResponseMessage> RsvpAsync(int eventId, int userId)
    {
        // Construct the request body as JSON string
        var content = new StringContent($"{{\"EventId\": {eventId}, \"UserId\": {userId}}}", Encoding.UTF8, "application/json");

        // Set content type header
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        // Send the POST request
        var response = await _httpClient.PostAsync($"api/events/rsvp", content);
        return response;
    }
}
