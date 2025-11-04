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
    
    if (builder.HostEnvironment.IsDevelopment())
    {
        // Local development - point to local API
        httpClient.BaseAddress = new Uri("http://localhost:5000");
    }
    else
    {
        // Production - use API_URL from configuration or default
        var apiUrl = builder.Configuration["API_URL"] ?? "https://campusconnectapp.delightfulsky-c3ec110b.uksouth.azurecontainerapps.io";
        Console.WriteLine($"[DEBUG] API URL: {apiUrl}");
        httpClient.BaseAddress = new Uri(apiUrl);
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
builder.Services.AddScoped<UserApi>();

await builder.Build().RunAsync().ConfigureAwait(false);