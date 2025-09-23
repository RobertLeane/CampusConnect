using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace CampusConnect.Api.Services
{
    public static class SqlService
    {
        public static async Task<SqlConnection> OpenTokenConnectionAsync(string server, string database)
        {
            var cs = $"Server=tcp:{server},1433;Database={database};Encrypt=True;TrustServerCertificate=False;";

            var scopes = new[] { "https://database.windows.net/.default" };
            var cred = new DefaultAzureCredential();

            AccessToken token = await cred.GetTokenAsync(new TokenRequestContext(scopes));

            var conn = new SqlConnection(cs)
            {
                AccessToken = token.Token
            };
            await conn.OpenAsync();
            return conn;
        }
    }
}
