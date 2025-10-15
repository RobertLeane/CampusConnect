// api/Services/SqlService.cs
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CampusConnect.Api.Services;

public static class SqlService
{
    // Scope for Azure SQL
    private static readonly string[] SqlScopes = new[] { "https://database.windows.net/.default" };

    public static async Task<SqlConnection> OpenTokenConnectionAsync(string server, string database, IConfiguration? configuration = null)
    {
        // For local development, try connection string first
        string? connectionString = null;

        // Get connection string using standard ASP.NET Core configuration
        if (configuration != null)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Fallback to environment variables
        connectionString ??= Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        
        Console.WriteLine($"Checking connection string from config...");
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine($"Found connection string (first 50 chars): {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
        }
        else
        {
            Console.WriteLine("No connection string found in config or environment variables");
        }
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Using connection string authentication");
            try
            {
                var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                Console.WriteLine("Connection string authentication successful");
                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection string authentication failed: {ex.Message}");
                // Continue to token auth fallback
            }
        }

        Console.WriteLine("Falling back to token authentication");
        try
        {
            // Fallback to token authentication for Azure
            var cs = $"Server=tcp:{server},1433;Database={database};Encrypt=True;TrustServerCertificate=False;";
            var tokenConn = new SqlConnection(cs);

            // Acquire AAD token
            var credential = new DefaultAzureCredential();
            AccessToken token = await credential.GetTokenAsync(new TokenRequestContext(SqlScopes), default);

            tokenConn.AccessToken = token.Token;
            await tokenConn.OpenAsync();
            Console.WriteLine("Token authentication successful");
            return tokenConn;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token authentication failed: {ex.Message}");
            throw new InvalidOperationException($"Unable to connect to database. Connection string auth failed and token auth failed: {ex.Message}");
        }
    }

    // Convenience factory that reads from configuration and returns an open connection
    public static Task<SqlConnection> CreateConnection(IConfiguration configuration)
    {
        var server = configuration["Sql:Server"]
                   ?? Environment.GetEnvironmentVariable("SQL_SERVER")
                   ?? throw new InvalidOperationException("Sql:Server is not configured.");

        var database = configuration["Sql:Database"]
                     ?? Environment.GetEnvironmentVariable("SQL_DATABASE")
                     ?? throw new InvalidOperationException("Sql:Database is not configured.");

        return OpenTokenConnectionAsync(server, database, configuration);
    }

    // Legacy method for backward compatibility
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
