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

    // Try to start a new investment
    bool TryStartInvestment(
    string optionId,
    out ActiveInvestment? investment,
    out string? errorCode,
    out string? errorMessage
);

    // Complete an investment
    void CompleteInvestment(string activeInvestmentId);
}