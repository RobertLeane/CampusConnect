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
    
    // For App Service, the API and client are served from the same domain
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    
    return httpClient;
});

// Register API services
builder.Services.AddScoped<ClubApi>();
builder.Services.AddScoped<EventApi>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MemberApi>();

await builder.Build().RunAsync();