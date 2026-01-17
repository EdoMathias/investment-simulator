using InvestmentServer.Domain;
using InvestmentServer.Storage;

namespace InvestmentServer.Services;

public sealed class InvestmentService
{
    private readonly IAccountStore _store;

    // Constructor
    public InvestmentService(IAccountStore store)
    {
        _store = store;
    }

    public InvestResult TryInvest(string optionId)
    {
        var investmentResult = _store.TryStartInvestment(
            optionId,
            out ActiveInvestment? investment,
            out string? errorCode,
            out string? errorMessage
        );

        // If the investment was started successfully, return the investment
        if (investmentResult is true && investment is not null)
            return new InvestResult.Ok(investment);

        // If the investment was not started successfully, return the error
        return new InvestResult.Fail(
            errorCode ?? "INVEST_FAILED",
            errorMessage ?? "Failed to start investment."
        );
    }
}

// Result for the investment service
public abstract record InvestResult
{
    public sealed record Ok(ActiveInvestment Investment) : InvestResult;
    public sealed record Fail(string Code, string Message) : InvestResult;
}
