using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();


builder.Services.AddHttpClient("api", c =>
{
    c.BaseAddress = new Uri("https://jolly-forest-05a7e0503.2.azurestaticapps.net/api/");
});

builder.Build().Run();
