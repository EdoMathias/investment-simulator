using System.Text.Json;
using InvestmentServer.Domain;
using Microsoft.Extensions.Logging;
using System.Threading;
using InvestmentServer.Events;

namespace InvestmentServer.Storage;

/// <summary>
/// JSON file-based implementation of IAccountStore
/// For simplicity, this implementation uses a single JSON file to store all user accounts.
/// </summary>
public sealed class JsonFileAccountStore : IAccountStore
{
    private readonly ILogger<JsonFileAccountStore> _logger;
    private readonly string _path;

    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _currentUserName;

    private readonly List<InvestmentOption> _investmentOptions = new()
    {
        new InvestmentOption("short", "Short-term Investment", 10m, 20m, 10),
        new InvestmentOption("medium", "Medium-term Investment", 100m, 250m, 30),
        new InvestmentOption("long", "Long-term Investment", 1000m, 3000m, 60),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonFileAccountStore(string path, ILogger<JsonFileAccountStore> logger, CompletionEventsHub eventsHub)
    {
        _path = path;
        _logger = logger;
        _eventsHub = eventsHub;
    }

    private readonly CompletionEventsHub _eventsHub;

    // --------- IAccountStore ---------

    public async Task<AccountState> GetAccountStateAsync(CancellationToken ct = default)
    {
        if (_currentUserName is null)
        {
            // Not logged in, return empty state
            return new AccountState("", 0m, Array.Empty<ActiveInvestment>(), Array.Empty<InvestmentHistoryItem>());
        }

        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            var user = GetOrCreateUserUnsafe(db, _currentUserName);

            // Ensure persisted if newly created
            await WriteDbUnsafeAsync(db, ct);

            _logger.LogDebug("Retrieved account state for user {User}", _currentUserName);

            return new AccountState(
                user.UserName,
                user.Balance,
                user.ActiveInvestments.ToList(),
                user.InvestmentHistory.ToList()
            );
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<InvestmentHistoryItem>> GetHistoryAsync(CancellationToken ct = default)
    {
        if (_currentUserName is null)
        {
            return new List<InvestmentHistoryItem>();
        }

        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            var user = GetOrCreateUserUnsafe(db, _currentUserName);

            // Ensure persisted if newly created
            await WriteDbUnsafeAsync(db, ct);

            _logger.LogDebug("Retrieved investment history for user {User}", _currentUserName);

            return user.InvestmentHistory
                .OrderByDescending(i => i.CompletedAtUtc)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task<IReadOnlyList<InvestmentOption>> GetInvestmentOptionsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieved investment options");
        return Task.FromResult((IReadOnlyList<InvestmentOption>)_investmentOptions.ToList());
    }

    public Task LoginAsync(string userName, CancellationToken ct = default)
    {
        _currentUserName = userName;
        _logger.LogInformation("User {User} logged in", userName);
        return Task.CompletedTask;
    }

    public Task LogoutAsync(CancellationToken ct = default)
    {

        _logger.LogInformation("User {User} logged out", _currentUserName);
        _currentUserName = null;
        return Task.CompletedTask;
    }

    public async Task<InvestResult<ActiveInvestment>> TryStartInvestmentAsync(string optionId, CancellationToken ct = default)
    {
        _logger.LogDebug("User {User} is attempting to start investment with option {OptionId}", _currentUserName, optionId);

        if (_currentUserName is null)
            return InvestResult<ActiveInvestment>.Failure("NOT_LOGGED_IN", "User is not logged in");

        var option = _investmentOptions.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            return InvestResult<ActiveInvestment>.Failure("INVALID_OPTION", "Invalid investment option");

        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);

            var user = GetOrCreateUserUnsafe(db, _currentUserName);

            if (user.Balance < option.RequiredAmount)
                return InvestResult<ActiveInvestment>.Failure("INSUFFICIENT_BALANCE", "Not enough balance for this investment");

            if (user.ActiveInvestments.Any(i => i.OptionId == optionId))
                return InvestResult<ActiveInvestment>.Failure("INVESTMENT_ALREADY_ACTIVE", "Investment is already active");

            user.Balance -= option.RequiredAmount;

            var now = DateTime.UtcNow;

            var investment = new ActiveInvestment(
                Id: Guid.NewGuid().ToString(),
                OptionId: optionId,
                Name: option.Name,
                InvestedAmount: option.RequiredAmount,
                ExpectedReturn: option.ExpectedReturn,
                StartTimeUtc: now,
                EndTimeUtc: now.AddSeconds(option.DurationSeconds)
            );

            user.ActiveInvestments.Add(investment);

            await WriteDbUnsafeAsync(db, ct);

            _logger.LogDebug("User {User} started investment with option {OptionId}, amount {Amount}. New balance: {Balance}", _currentUserName, optionId, option.RequiredAmount, user.Balance);

            return InvestResult<ActiveInvestment>.Success(investment);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<ActiveInvestment>> GetAllActiveInvestmentsAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            return db.Users.Values
                .SelectMany(u => u.ActiveInvestments)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<long?> CompleteInvestmentAsync(string activeInvestmentId, CancellationToken ct = default)
    {
        InvestmentCompletedEvent? eventToPublish = null;

        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            var now = DateTime.UtcNow;

            foreach (var user in db.Users.Values)
            {
                var index = user.ActiveInvestments.FindIndex(i => i.Id == activeInvestmentId);
                if (index < 0) continue;

                var investment = user.ActiveInvestments[index];
                if (investment.EndTimeUtc > now) return null;

                user.Balance += investment.ExpectedReturn;

                user.InvestmentHistory.Add(new InvestmentHistoryItem(
                    Id: investment.Id,
                    OptionId: investment.OptionId,
                    Name: investment.Name,
                    InvestedAmount: investment.InvestedAmount,
                    ReturnedAmount: investment.ExpectedReturn,
                    StartTimeUtc: investment.StartTimeUtc,
                    EndTimeUtc: investment.EndTimeUtc,
                    CompletedAtUtc: now
                ));

                user.ActiveInvestments.RemoveAt(index);

                await WriteDbUnsafeAsync(db, ct);

                _logger.LogInformation(
                    "Investment completed. User={User} InvestmentId={InvestmentId} Returned={Returned} BalanceAfter={BalanceAfter}",
                    user.UserName, investment.Id, investment.ExpectedReturn, user.Balance
                );

                eventToPublish = new InvestmentCompletedEvent(
                    Token: 0,
                    UserName: user.UserName,
                    InvestmentId: investment.Id,
                    OptionId: investment.OptionId,
                    Name: investment.Name,
                    InvestedAmount: investment.InvestedAmount,
                    ReturnedAmount: investment.ExpectedReturn,
                    BalanceAfter: user.Balance,
                    StartTimeUtc: investment.StartTimeUtc,
                    EndTimeUtc: investment.EndTimeUtc,
                    CompletedAtUtc: now
                );

                break;
            }
        }
        finally
        {
            _gate.Release();
        }

        if (eventToPublish is null) return null;

        var published = _eventsHub.Publish(eventToPublish);

        _logger.LogDebug(
            "Published completion event. Token={Token} User={User} InvestmentId={InvestmentId}",
            published.Token, published.UserName, published.InvestmentId
        );

        return published.Token;
    }

    // --------- File IO and Helpers (caller must hold _gate) ---------

    /// <summary>
    /// Get existing user or create a new one (unsafe, caller must hold _gate)
    /// </summary>
    /// <returns>UserAccount</returns>
    private static UserAccount GetOrCreateUserUnsafe(AccountsDb db, string userName)
    {
        if (!db.Users.TryGetValue(userName, out var user))
        {
            user = new UserAccount
            {
                UserName = userName,
                Balance = 1000m,
                ActiveInvestments = new(),
                InvestmentHistory = new()
            };

            db.Users[userName] = user;
        }

        return user;
    }

    /// <summary>
    /// Read the accounts DB from the JSON file (unsafe, caller must hold _gate)
    /// </summary>
    private async Task<AccountsDb> ReadDbUnsafeAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_path))
            return new AccountsDb();

        await using var fs = new FileStream(
            _path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync<AccountsDb>(fs, JsonOptions, ct)
               ?? new AccountsDb();
    }

    /// <summary>
    /// Write the accounts DB to the JSON file (unsafe, caller must hold _gate)
    /// </summary>
    private async Task WriteDbUnsafeAsync(AccountsDb db, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var tmp = _path + ".tmp";

        await using (var fs = new FileStream(
            tmp, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true))
        {
            await JsonSerializer.SerializeAsync(fs, db, JsonOptions, ct);
            await fs.FlushAsync(ct);
        }

        File.Copy(tmp, _path, overwrite: true);
        File.Delete(tmp);
    }

    // --------- DB records ---------

    /// <summary>
    /// Accounts database model
    /// </summary>
    private sealed class AccountsDb
    {
        public Dictionary<string, UserAccount> Users { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// User account model
    /// </summary>
    private sealed class UserAccount
    {
        public string UserName { get; set; } = "";
        public decimal Balance { get; set; } = 1000m;
        public List<ActiveInvestment> ActiveInvestments { get; set; } = new();
        public List<InvestmentHistoryItem> InvestmentHistory { get; set; } = new();
    }
}
