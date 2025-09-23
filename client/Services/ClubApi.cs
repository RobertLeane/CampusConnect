using System.Net.Http.Json;

namespace Client.Services;

public class ClubDto
{
    public int ClubId { get; set; }
    public string ClubName { get; set; } = "";
    public string? Description { get; set; }
    public bool Active { get; set; }
}

public class ClubApi(HttpClient http)
{
    public async Task<List<ClubDto>> GetClubsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<ClubDto>>("/api/clubs", ct) ?? [];
}
