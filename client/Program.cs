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
    
    // In production (Azure Static Web Apps), use the current host
    // In development, use the SWA emulator
    if (builder.HostEnvironment.IsProduction())
    {
        httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    }
    else
    {
        httpClient.BaseAddress = new Uri("http://localhost:4280");
    }
    
    return httpClient;
});

// Register API services
builder.Services.AddScoped<UserApi>();
builder.Services.AddScoped<ClubApi>();
builder.Services.AddScoped<EventApi>();

await builder.Build().RunAsync();