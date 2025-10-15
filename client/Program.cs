using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient();
    
    // For local development, point to the API running on port 5000
    // For App Service deployment, both client and API will be on the same domain
    if (builder.HostEnvironment.IsDevelopment())
    {
        httpClient.BaseAddress = new Uri("http://localhost:5000");
    }
    else
    {
        // For App Service, the API and client are served from the same domain
        // Use the current page's base address
        var baseAddress = builder.HostEnvironment.BaseAddress;
        if (string.IsNullOrEmpty(baseAddress))
        {
            // Fallback: construct from current location
            baseAddress = $"{Uri.UriSchemeHttps}://{System.Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME") ?? "localhost"}";
        }
        httpClient.BaseAddress = new Uri(baseAddress);
    }
    
    return httpClient;
});

// Register API services
builder.Services.AddScoped<ClubApi>();
builder.Services.AddScoped<EventApi>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MemberApi>();
builder.Services.AddScoped<EventParticipantApi>();
builder.Services.AddScoped<EventRegistrationService>();

await builder.Build().RunAsync();