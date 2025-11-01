using CampusConnect.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources - works for both local and Azure
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

// Configure logging for better diagnostics
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure ports: Azure App Service uses PORT env var, local dev uses launch settings
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PORT")))
{
    // Azure App Service - use the PORT environment variable
    var port = Environment.GetEnvironmentVariable("PORT");
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
// For local development, let ASP.NET Core use the default ports from launchSettings.json

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CampusConnect API", Version = "v1" });
});

// Register your API services (with error handling)
try 
{
    builder.Services.AddScoped<IClubService, ClubService>();
    builder.Services.AddScoped<IEventService, EventService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IEventParticipantService, EventParticipantService>();
    builder.Services.AddScoped<IMemberService, MemberService>();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Warning: Service registration failed: {ex.Message}");
    throw;
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Add startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== CampusConnect API Starting ===");
logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
logger.LogInformation($"Port: {Environment.GetEnvironmentVariable("PORT") ?? "8080"}");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CampusConnect API V1");
        c.RoutePrefix = "swagger";
    });
    // Only use HTTPS redirection in development
    app.UseHttpsRedirection();
}
else
{
    // In production, serve Swagger at /swagger but don't make it the default
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CampusConnect API V1");
        c.RoutePrefix = "swagger";
    });
    // Don't use HTTPS redirection in production (Azure App Service handles this)
}
app.UseCors("AllowAll");
app.UseAuthorization();

// ✅ Serve Blazor WASM + Controllers
app.UseStaticFiles();
app.UseRouting();

// Add logging middleware to debug requests
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
    await next().ConfigureAwait(false);
    logger.LogInformation($"Response: {context.Response.StatusCode}");
});

// Add a simple health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Add debug endpoints for Azure App Service
app.MapGet("/debug/env", () => new 
{ 
    environment = app.Environment.EnvironmentName,
    port = Environment.GetEnvironmentVariable("PORT"),
    urls = builder.WebHost.GetSetting("urls"),
    contentRoot = app.Environment.ContentRootPath,
    webRoot = app.Environment.WebRootPath
});

app.MapGet("/debug/files", () => 
{
    var wwwrootPath = app.Environment.WebRootPath;
    if (!string.IsNullOrEmpty(wwwrootPath) && Directory.Exists(wwwrootPath))
    {
        var files = Directory.GetFiles(wwwrootPath, "*", SearchOption.AllDirectories)
            .Select(f => f.Replace(wwwrootPath, "").Replace("\\", "/"))
            .ToArray();
        return Results.Json(new { wwwrootExists = true, files });
    }
    return Results.Json(new { wwwrootExists = false, message = "wwwroot directory not found" });
});

// Debug endpoint to check database tables
app.MapGet("/debug/tables", async () => 
{
    try
    {
        using var conn = await CampusConnect.Api.Services.SqlService.CreateConnectionAsync(app.Services.GetRequiredService<IConfiguration>()).ConfigureAwait(false);
        using var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", conn);
        using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        
        var tables = new List<string>();
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            tables.Add(reader.GetString(0));
        }
        return Results.Json(new { tables });
    }
    catch (Microsoft.Data.SqlClient.SqlException ex)
    {
        return Results.Json(new { error = ex.Message });
    }
});

app.MapControllers();

// Serve the Blazor app for all non-API routes
app.MapFallbackToFile("index.html");

logger.LogInformation("=== CampusConnect API Configuration Complete ===");
logger.LogInformation("Available endpoints: /health, /api/*, /swagger, / (Blazor app)");

app.Run();
