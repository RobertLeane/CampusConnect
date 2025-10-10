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
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                            ?? Environment.GetEnvironmentVariable("DefaultConnection");
        
        Console.WriteLine($"Looking for connection string: {connectionString?.Substring(0, Math.Min(50, connectionString?.Length ?? 0))}...");
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Using connection string authentication");
            var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            return conn;
        }

        Console.WriteLine("Falling back to token authentication");
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

    // Convenience factory that reads env vars and returns an open connection
    public static Task<SqlConnection> CreateConnection()
    {
        var server = Environment.GetEnvironmentVariable("Sql__Server")
                   ?? Environment.GetEnvironmentVariable("SQL_SERVER")
                   ?? throw new InvalidOperationException("Sql__Server is not configured.");

        var database = Environment.GetEnvironmentVariable("Sql__Database")
                     ?? Environment.GetEnvironmentVariable("SQL_DATABASE")
                     ?? throw new InvalidOperationException("Sql__Database is not configured.");

        return OpenTokenConnectionAsync(server, database);
    }
}
