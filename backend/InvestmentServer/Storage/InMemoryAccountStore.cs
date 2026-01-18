using InvestmentServer.Domain;

namespace InvestmentServer.Storage;

// In-memory implementation of the account store
// This implementation is used to store the account state
// and investment options in memory
// and to start and complete investments in memory
public class InMemoryAccountStore : IAccountStore
{

    // Lock for thread safety
    private readonly object _lock = new();

    // The current user
    private string? _currentUserName;

    // User's balance
    private decimal _balance = 1000m;

    // User's active investments
    private readonly List<ActiveInvestment> _activeInvestments = new();

    // User's history of investments
    private readonly List<InvestmentHistoryItem> _investmentHistory = new();

    // List of investment options
    private readonly List<InvestmentOption> _investmentOptions = new() {
        new InvestmentOption("short", "Short-term Investment", 10m, 20m, 10),
        new InvestmentOption("medium", "Medium-term Investment", 100m, 250m, 30),
        new InvestmentOption("long", "Long-term Investment", 1000m, 3000m, 60),
    };

    /// <summary>
    /// Get the current account state
    /// </summary>
    /// <returns>The current account state</returns>
    public AccountState GetAccountState()
    {
        lock (_lock)
        {
            return new AccountState(_currentUserName, _balance, _activeInvestments.ToList(), _investmentHistory.ToList());
        }
    }

    /// <summary>
    /// Get the user's investment history
    /// </summary>
    /// <returns>A list of the user's investment history items</returns>
    public IReadOnlyList<InvestmentHistoryItem> GetInvestmentHistory()
    {
        lock (_lock)
        {
            return _investmentHistory.OrderByDescending(i => i.CompletedAtUtc).ToList();
        }
    }

    /// <summary>
    /// Get the investment options
    /// </summary>
    /// <returns>A list of the investment options</returns>
    public IReadOnlyList<InvestmentOption> GetInvestmentOptions()
    {
        lock (_lock)
        {
            return _investmentOptions.ToList();
        }
    }

    /// <summary>
    /// Set the current user
    /// </summary>
    /// <param name="userName">The user name</param>
    public void SetCurrentUser(string userName)
    {
        lock (_lock)
        {
            _currentUserName = userName;
        }
    }

    /// <summary>
    /// Clear the current user - log out
    /// </summary>
    public void ClearCurrentUser()
    {
        lock (_lock)
        {
            _currentUserName = null;
            _balance = 1000m;
            _activeInvestments.Clear();
            _investmentHistory.Clear();
        }
    }

    /// <summary>
    /// Try to start a new investment
    /// </summary>
    /// <param name="optionId"></param>
    /// <returns>Result of the investment attempt</returns>
    public InvestResult<ActiveInvestment> TryStartInvestment(string optionId)
    {
        lock (_lock)
        {
            // Check if the user is logged in
            // TODO: Move this to a helper method
            if (_currentUserName is null)
            {
                return InvestResult<ActiveInvestment>.Failure("NOT_LOGGED_IN", "User is not logged in");
            }

            // Check if the option exists
            // TODO: Move this to a helper method
            var option = _investmentOptions.FirstOrDefault(o => o.Id == optionId);
            if (option is null)
            {
                return InvestResult<ActiveInvestment>.Failure("INVALID_OPTION", "Invalid investment option");
            }

            // Check if the user has enough balance
            // TODO: Move this to a helper method
            if (_balance < option.RequiredAmount)
            {
                return InvestResult<ActiveInvestment>.Failure("INSUFFICIENT_BALANCE", "Not enough balance for this investment");
            }

            // Check if the investment is already active
            // TODO: Move this to a helper method
            if (_activeInvestments.Any(i => i.OptionId == optionId))
            {
                return InvestResult<ActiveInvestment>.Failure("INVESTMENT_ALREADY_ACTIVE", "Investment is already active");
            }

            // Reduce balance
            _balance -= option.RequiredAmount;

            // Create a new investment
            var investment = new ActiveInvestment(
                Guid.NewGuid().ToString(),
                optionId,
                option.Name,
                option.RequiredAmount,
                option.ExpectedReturn,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(option.DurationSeconds));

            // Add the investment to the active investments
            _activeInvestments.Add(investment);
            return InvestResult<ActiveInvestment>.Success(investment);
        }
    }

    /// <summary>
    /// Complete an investment
    /// </summary>
    /// <param name="activeInvestmentId">The ID of the active investment to complete</param>
    public void CompleteInvestment(string activeInvestmentId)
    {
        // TODO: Why lock all?
        lock (_lock)
        {
            var index = _activeInvestments.FindIndex(i => i.Id == activeInvestmentId);
            if (index < 0)
            {
                return;
            }

            var investment = _activeInvestments[index];
            var now = DateTime.UtcNow;

            // Return if the investment has not ended yet
            if (investment.EndTimeUtc > now) return;

            // Add investment return to balance
            _balance += investment.ExpectedReturn;

            // TODO: Add the investment to the history
            _investmentHistory.Add(new InvestmentHistoryItem(
                Id: investment.Id,
                OptionId: investment.OptionId,
                Name: investment.Name,
                InvestedAmount: investment.InvestedAmount,
                ReturnedAmount: investment.ExpectedReturn,
                StartTimeUtc: investment.StartTimeUtc,
                EndTimeUtc: investment.EndTimeUtc,
                CompletedAtUtc: now));

            // Remove the investment from the active investments
            _activeInvestments.RemoveAt(index);
        }
    }
}