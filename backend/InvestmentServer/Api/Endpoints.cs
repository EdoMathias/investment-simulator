
using InvestmentServer.Storage;
using InvestmentServer.Services;
using InvestmentServer.Contracts;
using InvestmentServer.Utils;

namespace InvestmentServer.Api;

public static class Endpoints
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {

        // GET: Health check
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

        // POST: Login
        app.MapPost("/api/login", (IAccountStore accountStore, LoginRequest request) =>
        {
            if (Validations.IsValidUserName(request.UserName) is false)
            {
                return ApiErrors.BadRequest("INVALID_USER_NAME", "User name must be 3-20 characters long and contain only English letters");
            }

            accountStore.SetCurrentUser(request.UserName);
            return ApiResults.Ok(new { message = "Logged in successfully. Hello, " + request.UserName + "!" });
        });

        // GET: Account State
        app.MapGet("/api/state", (IAccountStore accountStore) =>
        {
            var state = accountStore.GetAccountState();

            if (state is null)
            {
                // User is not logged in
                return ApiErrors.BadRequest("NOT_LOGGED_IN", "User is not logged in");
            }

            return ApiResults.Ok(state);
        });

        // GET: Investment Options
        app.MapGet("/api/investment-options", (IAccountStore accountStore) =>
        {
            return ApiResults.Ok(accountStore.GetInvestmentOptions());
        });

        // GET: Account's investment history
        app.MapGet("/api/investment-history", (IAccountStore accountStore) =>
        {
            return ApiResults.Ok(accountStore.GetInvestmentHistory());
        });

        // POST: Start an investment
        app.MapPost("/api/invest", (InvestmentService investmentService, InvestRequest request) =>
        {
            var result = investmentService.TryInvest(request.OptionId);
            return result switch
            {
                InvestResult.Ok ok =>
                    ApiResults.Ok(new { message = "Investment started successfully. Your investment will be completed in " + Math.Round(ok.Investment.EndTimeUtc.Subtract(DateTime.UtcNow).TotalSeconds) + " seconds." }),

                InvestResult.Fail fail =>
                    ApiErrors.BadRequest(fail.Code, fail.Message),

                _ => Results.StatusCode(500)
            };
        });

        return app;
    }
}