using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("http://localhost:7071") });

// Register API services
builder.Services.AddScoped<UserApi>();
builder.Services.AddScoped<ClubApi>();
builder.Services.AddScoped<EventApi>();

await builder.Build().RunAsync();