using InvestmentServer.Domain;

namespace InvestmentServer.Storage;

// Interface for the account store
// This interface is used to store and retrieve the account state
// and investment options
// and to start and complete investments
public interface IAccountStore
{
    Task<AccountState> GetAccountStateAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InvestmentHistoryItem>> GetHistoryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<InvestmentOption>> GetInvestmentOptionsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ActiveInvestment>> GetAllActiveInvestmentsAsync(CancellationToken ct = default);

    Task LoginAsync(string userName, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);

    Task<InvestResult<ActiveInvestment>> TryStartInvestmentAsync(string optionId, CancellationToken ct = default);

    Task<long?> CompleteInvestmentAsync(string activeInvestmentId, CancellationToken ct = default);
}