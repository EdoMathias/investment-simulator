using Microsoft.AspNetCore.StaticFiles;

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

// Build the web application
var app = builder.Build();

// Use CORS
app.UseCors();

// Serve static files from the wwwroot folder
app.UseDefaultFiles();
app.UseStaticFiles();

// Define a route for the ping endpoint
app.MapPost("/api/ping", (PingRequest body) =>
{
    // Log the request
    Console.WriteLine($"Ping request received: {body.name} said: {body.text}");

    // Make a response
    var response = new PingResponse(
        message: $"Hello, {body.name}! You said: {body.text}",
        timestamp: DateTime.UtcNow
    );

    // Return the response
    return Results.Ok(response);
});


// Run the web application
app.Run();


// Record types for the ping endpoint
record PingRequest(string name, string text);
record PingResponse(string message, DateTime timestamp);