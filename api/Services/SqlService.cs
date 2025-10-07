// api/Services/SqlService.cs
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace CampusConnect.Api.Services;

public static class SqlService
{
    // Scope for Azure SQL
    private static readonly string[] SqlScopes = new[] { "https://database.windows.net/.default" };

    public static async Task<SqlConnection> OpenTokenConnectionAsync(string server, string database)
    {
        // For local development, try connection string first
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }

        // Fallback to token authentication for Azure
        var cs = $"Server=tcp:{server},1433;Database={database};Encrypt=True;TrustServerCertificate=False;";
        var tokenConn = new SqlConnection(cs);

        // Acquire AAD token
        var credential = new DefaultAzureCredential();
        AccessToken token = await credential.GetTokenAsync(new TokenRequestContext(SqlScopes), default);

        tokenConn.AccessToken = token.Token;
        await tokenConn.OpenAsync();
        return tokenConn;
    }
}
