using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CampusConnect.Api.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register your API services
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IEventService, EventService>();

// TODO: Register the remaining services by uncommenting these lines as team members implement them:
// builder.Services.AddScoped<IMemberService, MemberService>();
// builder.Services.AddScoped<IEventParticipantService, EventParticipantService>();

builder.Build().Run();
