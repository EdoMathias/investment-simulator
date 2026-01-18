using InvestmentServer.Domain;

namespace InvestmentServer.Storage;

// Interface for the account store
// This interface is used to store and retrieve the account state
// and investment options
// and to start and complete investments
public interface IAccountStore
{
    // Get the current account state
    AccountState GetAccountState();

    // Get the investment options
    IReadOnlyList<InvestmentOption> GetInvestmentOptions();

    // Get the investment history
    IReadOnlyList<InvestmentHistoryItem> GetInvestmentHistory();

    // Set current user
    void SetCurrentUser(string userName);

    // Clear current user
    void ClearCurrentUser();

    // Try to start a new investment
    Task<InvestResult<ActiveInvestment>> TryStartInvestment(string optionId);

    // Complete an investment
    Task CompleteInvestment(string activeInvestmentId, CancellationToken ct = default);

    // Get all active investments
    Task<IReadOnlyList<ActiveInvestment>> GetAllActiveInvestmentsAsync(CancellationToken ct = default);
}