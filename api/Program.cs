using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CampusConnect.Api.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5094")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register your API services
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IEventService, EventService>();

// TODO: Register the remaining services by uncommenting these lines as team members implement them:
// builder.Services.AddScoped<IEventService, EventService>();

// builder.Services.AddScoped<IMemberService, MemberService>();
// builder.Services.AddScoped<IEventParticipantService, EventParticipantService>();

builder.Build().Run();
