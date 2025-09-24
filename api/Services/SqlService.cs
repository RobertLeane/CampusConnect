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
        // Minimal, valid base connection string (no auth details here)
        var cs = $"Server=tcp:{server},1433;Database={database};Encrypt=True;TrustServerCertificate=False;";

        var conn = new SqlConnection(cs);

        // Acquire AAD token
        var credential = new DefaultAzureCredential(); // uses VS/CLI login locally; Managed Identity in Azure
        AccessToken token = await credential.GetTokenAsync(new TokenRequestContext(SqlScopes), default);

        conn.AccessToken = token.Token;
        await conn.OpenAsync();
        return conn;
    }
}
