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
        var investmentResult = _store.TryStartInvestment(optionId);

        // If investment was not successful, return the error
        if (investmentResult.IsSuccess is false)
        {
            return new InvestResult.Fail(
                investmentResult.Error!.Code,
                investmentResult.Error.Message
            );
        }

        return new InvestResult.Ok(investmentResult.Data!);
    }
}

// Result for the investment service
public abstract record InvestResult
{
    public sealed record Ok(ActiveInvestment Investment) : InvestResult;
    public sealed record Fail(string Code, string Message) : InvestResult;
}
