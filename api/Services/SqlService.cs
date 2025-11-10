// api/Services/SqlService.cs
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CampusConnect.Api.Services;

public static class SqlService
{
    private static readonly string[] SqlScopes = new[] { "https://database.windows.net/.default" };

    public static async Task<SqlConnection> OpenTokenConnectionAsync(string server, string database, IConfiguration? configuration = null)
    {
        string? connectionString = null;

        // appsettings.json
        if (configuration != null)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        // Custom environment variables
        connectionString ??= Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // local.settings.json for local development
        if (configuration != null && string.IsNullOrEmpty(connectionString))
        {
            var localConnectionString = configuration["ConnectionStrings:DefaultConnection"];
            connectionString = localConnectionString;
        }
        
        Console.WriteLine($"Checking connection string from config...");
        if (!string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine($"Found connection string (first 50 chars): {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            try
            {
                var conn = new SqlConnection(connectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                Console.WriteLine("Connection string authentication successful");
                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection string authentication failed: {ex.Message}");
                throw new InvalidOperationException($"Database connection failed: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No connection string found in any configuration source");
            throw new InvalidOperationException("No database connection string configured. Please add ConnectionStrings:DefaultConnection to appsettings.json or configure it in Azure App Service.");
        }
    }

    // Convenience factory that reads from configuration and returns an open connection
    public static Task<SqlConnection> CreateConnectionAsync(IConfiguration configuration)
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
    public static Task<SqlConnection> CreateConnectionAsync()
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
