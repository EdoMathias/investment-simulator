using InvestmentServer.Domain;

namespace InvestmentServer.Storage;

/// <summary>
/// Interface for account storage and management
/// Provides methods for login, logout, retrieving account state,
/// investment options, investment history, and managing investments.
/// </summary>
public interface IAccountStore
{
    /// <summary>
    /// Gets the current account state
    /// </summary>
    Task<AccountState> GetAccountStateAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the investment history
    /// </summary>
    Task<IReadOnlyList<InvestmentHistoryItem>> GetHistoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the available investment options
    /// </summary>
    Task<IReadOnlyList<InvestmentOption>> GetInvestmentOptionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all active investments
    /// </summary>
    Task<IReadOnlyList<ActiveInvestment>> GetAllActiveInvestmentsAsync(CancellationToken ct = default);

    /// <summary>
    /// Logs in a user
    /// </summary>
    Task LoginAsync(string userName, CancellationToken ct = default);

    /// <summary>
    /// Logs out the current user
    /// </summary>
    Task LogoutAsync(CancellationToken ct = default);

    /// <summary>
    /// Attempts to start a new investment
    /// </summary>
    Task<InvestResult<ActiveInvestment>> TryStartInvestmentAsync(string optionId, CancellationToken ct = default);

    /// <summary>
    /// Completes an active investment
    /// </summary>
    Task<long?> CompleteInvestmentAsync(string activeInvestmentId, CancellationToken ct = default);
}