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
        // For App Service, use the same domain - no subdirectory needed
        var baseUri = builder.HostEnvironment.BaseAddress;
        Console.WriteLine($"[DEBUG] Environment BaseAddress: {baseUri}");
        httpClient.BaseAddress = new Uri(baseUri);
        Console.WriteLine($"[DEBUG] HttpClient BaseAddress set to: {httpClient.BaseAddress}");
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

await builder.Build().RunAsync();