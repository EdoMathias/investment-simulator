using InvestmentServer.Domain;
using InvestmentServer.Storage;
using InvestmentServer.Workers;

namespace InvestmentServer.Services;

public sealed class InvestmentService
{
    private readonly IAccountStore _store;
    private readonly InvestmentCompletionScheduler _scheduler;

    public InvestmentService(IAccountStore store, InvestmentCompletionScheduler scheduler)
    {
        _store = store;
        _scheduler = scheduler;
    }

    public async Task<InvestResult> TryInvest(string optionId)
    {
        var investmentResult = await _store.TryStartInvestment(optionId);

        if (investmentResult.IsSuccess is false)
        {
            return new InvestResult.Fail(
                investmentResult.Error!.Code,
                investmentResult.Error.Message
            );
        }

        var investment = investmentResult.Data!;
        _scheduler.Schedule(investment);

        return new InvestResult.Ok(investment);
    }
}

// Result for the investment service
public abstract record InvestResult
{
    public sealed record Ok(ActiveInvestment Investment) : InvestResult;
    public sealed record Fail(string Code, string Message) : InvestResult;
}
