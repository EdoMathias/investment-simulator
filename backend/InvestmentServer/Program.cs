using Microsoft.AspNetCore.StaticFiles;
using InvestmentServer.Storage;
using InvestmentServer.Services;
using InvestmentServer.Workers;
using InvestmentServer.Api;

// Create a new web application builder
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

// Add the account store to the builder
builder.Services.AddSingleton<IAccountStore, InMemoryAccountStore>();
builder.Services.AddSingleton<InvestmentService>();

// // Add the investment processor to track the active investments and complete them when they are due
builder.Services.AddHostedService<InvestmentProcessor>();


// Build the web application
var app = builder.Build();

// Use CORS
app.UseCors();

// Map the API endpoints through the extension method
app.MapEndpoints();

app.Run();