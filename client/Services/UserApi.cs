using System.Net.Http.Json;
using Client.Models;

namespace Client.Services;

public class UserApi
{
    private readonly HttpClient _httpClient;

    public UserApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        try
        {
            Console.WriteLine("Calling UserApi.GetUsersAsync()");
            var response = await _httpClient.GetAsync("api/users/count");
            Console.WriteLine($"UserApi response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var count = await response.Content.ReadFromJsonAsync<int>();
                Console.WriteLine($"UserApi returned count: {count}");
                
                // Create a list with the right count for consistency with clubs/events pattern
                // We don't need actual user data, just the count, so create empty User objects
                var users = new List<User>();
                for (int i = 0; i < count; i++)
                {
                    users.Add(new User()); // Empty user object just for counting
                }
                return users;
            }
            else
            {
                Console.WriteLine($"UserApi error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return new List<User>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserApi exception: {ex.Message}");
            return new List<User>();
        }
    }
}