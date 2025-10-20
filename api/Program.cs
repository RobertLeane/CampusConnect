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
catch (Exception ex)
{
    Console.WriteLine($"Warning: Service registration failed: {ex.Message}");
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
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// ✅ Serve Blazor WASM + Controllers
app.UseStaticFiles();
app.UseRouting();

// Add a simple health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

app.MapControllers();
app.MapFallbackToFile("index.html"); // <-- critical: serves Blazor app at "/"

logger.LogInformation("=== CampusConnect API Configuration Complete ===");
logger.LogInformation("Available endpoints: /health, /api/*, /swagger, / (Blazor app)");

app.Run();
