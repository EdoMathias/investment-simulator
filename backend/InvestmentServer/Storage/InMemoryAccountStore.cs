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
    /// Try to start a new investment
    /// </summary>
    /// <param name="optionId">The ID of the investment option to start</param>
    /// <param name="investment">The investment that was started</param>
    /// <param name="errorCode">The error code if the investment could not be started</param>
    /// <param name="errorMessage">The error message if the investment could not be started</param>
    /// <returns>True if the investment was started successfully, false otherwise</returns>
    public bool TryStartInvestment(string optionId, out ActiveInvestment? investment, out string? errorCode, out string? errorMessage)
    {
        lock (_lock)
        {
            investment = null;
            errorCode = null;
            errorMessage = null;


            // Check if the user is logged in
            // TODO: Move this to a helper method
            if (_currentUserName is null)
            {
                errorCode = "NOT_LOGGED_IN";
                errorMessage = "User is not logged in";
                return false;
            }

            // Check if the option exists
            // TODO: Move this to a helper method
            var option = _investmentOptions.FirstOrDefault(o => o.Id == optionId);
            if (option is null)
            {
                errorCode = "INVALID_OPTION";
                errorMessage = "Invalid investment option";
                return false;
            }

            // Check if the user has enough balance
            // TODO: Move this to a helper method
            if (_balance < option.RequiredAmount)
            {
                errorCode = "INSUFFICIENT_BALANCE";
                errorMessage = "Not enough balance for this investment";
                return false;
            }

            // Check if the investment is already active
            // TODO: Move this to a helper method
            if (_activeInvestments.Any(i => i.OptionId == optionId))
            {
                errorCode = "INVESTMENT_ALREADY_ACTIVE";
                errorMessage = "Investment is already active";
                return false;
            }

            // Reduce balance
            _balance -= option.RequiredAmount;

            // Create a new investment
            investment = new ActiveInvestment(
                Guid.NewGuid().ToString(),
                optionId,
                option.Name,
                option.RequiredAmount,
                option.ExpectedReturn,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(option.DurationSeconds));

            // Add the investment to the active investments
            _activeInvestments.Add(investment);
            return true;
        }
    }

    /// <summary>
    /// Complete an investment
    /// </summary>
    /// <param name="activeInvestmentId">The ID of the active investment to complete</param>
    public void CompleteInvestment(string activeInvestmentId)
    {
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