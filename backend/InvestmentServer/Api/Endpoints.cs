
using InvestmentServer.Storage;
using InvestmentServer.Services;
using InvestmentServer.Contracts;
using InvestmentServer.Utils;
using InvestmentServer.Events;
using System.Text.Json;

namespace InvestmentServer.Api;

/// <summary>
/// Extension methods for mapping API endpoints
/// </summary>
public static class Endpoints
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {

        // GET: Health check
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

        // POST: Login
        app.MapPost("/api/login", async (IAccountStore accountStore, LoginRequest request) =>
        {
            if (Validations.IsValidUserName(request.UserName) is false)
            {
                return ApiErrors.BadRequest("INVALID_USER_NAME", "User name must be 3-20 characters long and contain only English letters");
            }

            await accountStore.LoginAsync(request.UserName);
            return ApiResults.Ok(new { message = "Logged in successfully. Hello, " + request.UserName + "!" });
        });

        // POST: Logout
        app.MapPost("/api/logout", async (IAccountStore accountStore) =>
        {
            await accountStore.LogoutAsync();
            return ApiResults.Ok(new { message = "Logged out successfully." });
        });

        // GET: Account State
        app.MapGet("/api/state", async (IAccountStore accountStore) =>
        {
            var state = await accountStore.GetAccountStateAsync();

            if (state is null)
            {
                // User is not logged in
                return ApiErrors.BadRequest("NOT_LOGGED_IN", "User is not logged in");
            }

            return ApiResults.Ok(state);
        });

        // GET: Investment Options
        app.MapGet("/api/investment-options", async (IAccountStore accountStore) =>
        {
            return ApiResults.Ok(await accountStore.GetInvestmentOptionsAsync());
        });

        // GET: Account's investment history
        app.MapGet("/api/investment-history", async (IAccountStore accountStore) =>
        {
            return ApiResults.Ok(await accountStore.GetHistoryAsync());
        });

        // POST: Start an investment
        app.MapPost("/api/invest", async (InvestmentService investmentService, InvestRequest request) =>
        {
            var result = await investmentService.TryInvest(request.OptionId);
            return result switch
            {
                InvestResult.Ok ok =>
                    ApiResults.Ok(new { message = "Investment started successfully. Your investment will be completed in " + Math.Round(ok.Investment.EndTimeUtc.Subtract(DateTime.UtcNow).TotalSeconds) + " seconds." }),

                InvestResult.Fail fail =>
                    ApiErrors.BadRequest(fail.Code, fail.Message),

                _ => Results.StatusCode(500)
            };
        });

        // SSE: Stream investment completion events
        app.MapGet("/events/completions/stream", async (HttpContext ctx, CompletionEventsHub hub) =>
        {
            ctx.Response.Headers["Content-Type"] = "text/event-stream";
            ctx.Response.Headers["Cache-Control"] = "no-cache";
            ctx.Response.Headers["Connection"] = "keep-alive";

            var (reader, unsubscribe) = hub.Subscribe();

            try
            {
                // Initial comment so client knows it's connected
                await ctx.Response.WriteAsync(": connected\n\n");
                await ctx.Response.Body.FlushAsync();

                var heartbeat = TimeSpan.FromSeconds(1);

                while (!ctx.RequestAborted.IsCancellationRequested)
                {
                    var readTask = reader.ReadAsync(ctx.RequestAborted).AsTask();
                    var pingTask = Task.Delay(heartbeat, ctx.RequestAborted);

                    var completed = await Task.WhenAny(readTask, pingTask);

                    if (completed == pingTask)
                    {
                        await ctx.Response.WriteAsync(": ping\n\n");
                        await ctx.Response.Body.FlushAsync();
                        continue;
                    }

                    var ev = await readTask;

                    await ctx.Response.WriteAsync($"id: {ev.Token}\n");
                    await ctx.Response.WriteAsync("event: investmentCompleted\n");
                    await ctx.Response.WriteAsync($"data: {JsonSerializer.Serialize(ev)}\n\n");
                    await ctx.Response.Body.FlushAsync();
                }
            }
            finally
            {
                unsubscribe();
            }
        });

        return app;
    }
}