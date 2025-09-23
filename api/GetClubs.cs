using Microsoft.Data.SqlClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using CampusConnect.Api.Services;

public class GetClubs
{
    [Function("GetClubs")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "clubs")] HttpRequestData req)
    {
        var server = Environment.GetEnvironmentVariable("Sql__Server");
        var database = Environment.GetEnvironmentVariable("Sql__Database");

        await using var conn = await SqlService.OpenTokenConnectionAsync(server!, database!);

        var cmd = new SqlCommand("SELECT Id, Name, Description FROM Club", conn);
        var reader = await cmd.ExecuteReaderAsync();

        var clubs = new List<object>();
        while (await reader.ReadAsync())
        {
            clubs.Add(new {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2)
            });
        }

        var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await res.WriteAsJsonAsync(clubs);
        return res;
    }
}
