using Microsoft.AspNetCore.StaticFiles;
using InvestmentServer.Storage;
using InvestmentServer.Services;
using InvestmentServer.Workers;
using InvestmentServer.Api;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services to the builder
builder.Services.AddCors((options) =>
{
    options.AddDefaultPolicy((policy) =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<InvestmentServer.Events.CompletionEventsHub>();

builder.Services.AddSingleton<IAccountStore>(sp =>
{
    var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "accounts.json");

    return new JsonFileAccountStore(
        dataPath,
        sp.GetRequiredService<ILogger<JsonFileAccountStore>>(),
        sp.GetRequiredService<InvestmentServer.Events.CompletionEventsHub>()
    );
});

builder.Services.AddSingleton<InvestmentCompletionScheduler>();
builder.Services.AddHostedService<InvestmentRescheduler>();

builder.Services.AddSingleton<InvestmentService>();

// Build the web application
var app = builder.Build();

// Use CORS
app.UseCors();

// Map the API endpoints through the extension method
app.MapEndpoints();

app.Run();