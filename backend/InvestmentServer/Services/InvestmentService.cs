using InvestmentServer.Domain;
using InvestmentServer.Storage;
using InvestmentServer.Workers;

namespace InvestmentServer.Services;

/// <summary>
/// Service for handling investments
/// </summary>
public sealed class InvestmentService
{
    private readonly IAccountStore _store;
    private readonly InvestmentCompletionScheduler _scheduler;
    private readonly ILogger<InvestmentService> _logger;

    public InvestmentService(IAccountStore store, InvestmentCompletionScheduler scheduler, ILogger<InvestmentService> logger)
    {
        _store = store;
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<InvestResult> TryInvest(string optionId)
    {
        var investmentResult = await _store.TryStartInvestmentAsync(optionId);

        if (investmentResult.IsSuccess is false)
        {
            return new InvestResult.Fail(
                investmentResult.Error!.Code,
                investmentResult.Error.Message
            );
        }

        var investment = investmentResult.Data!;
        _scheduler.Schedule(investment);
        _logger.LogInformation("Investment started: {InvestmentId} for option {OptionId}", investment.Id, optionId);

        return new InvestResult.Ok(investment);
    }
}

/// <summary>
/// Results of an investment attempt
/// </summary>
public abstract record InvestResult
{
    public sealed record Ok(ActiveInvestment Investment) : InvestResult;
    public sealed record Fail(string Code, string Message) : InvestResult;
}
