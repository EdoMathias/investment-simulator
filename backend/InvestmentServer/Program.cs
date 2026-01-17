using Microsoft.AspNetCore.StaticFiles;
using InvestmentServer.Storage;
using InvestmentServer.Services;
using InvestmentServer.Contracts;
using InvestmentServer.Utils;
using InvestmentServer.Workers;

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

// Define a route for the health endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

//------------------------------------------------------
// Routes for the API
//------------------------------------------------------

// POST: Login
app.MapPost("/api/login", (IAccountStore accountStore, LoginRequest request) =>
{
    if (Validations.IsValidUserName(request.UserName) is false)
    {
        return Results.BadRequest(new ApiError("INVALID_USER_NAME", "User name must be 3-20 characters long and contain only English letters"));
    }

    accountStore.SetCurrentUser(request.UserName);
    return Results.Ok(new { message = "Logged in successfully. Hello, " + request.UserName + "!" });
});

// GET: State
app.MapGet("/api/state", (IAccountStore accountStore) =>
{
    var state = accountStore.GetAccountState();

    if (state is null)
    {
        // User is not logged in
        // return Results.Unauthorized();
        return Results.BadRequest(new ApiError("NOT_LOGGED_IN", "User is not logged in"));
    }

    return Results.Ok(state);
});

// GET: Investment Options
app.MapGet("/api/investment-options", (IAccountStore accountStore) =>
{
    return Results.Ok(accountStore.GetInvestmentOptions());
});

// GET: Investment History
app.MapGet("/api/investment-history", (IAccountStore accountStore) =>
{
    return Results.Ok(accountStore.GetInvestmentHistory());
});

// POST: Start an investment
app.MapPost("/api/invest", (InvestmentService investmentService, InvestRequest request) =>
{
    var result = investmentService.TryInvest(request.OptionId);
    return result switch
    {
        InvestResult.Ok ok =>
            Results.Ok(new { message = "Investment started successfully. Your investment will be completed in " + Math.Round(ok.Investment.EndTimeUtc.Subtract(DateTime.UtcNow).TotalSeconds) + " seconds." }),

        InvestResult.Fail fail =>
            Results.BadRequest(new ApiError(fail.Code, fail.Message)),

        _ => Results.StatusCode(500)
    };
});
//------------------------------------------------------

app.Run();