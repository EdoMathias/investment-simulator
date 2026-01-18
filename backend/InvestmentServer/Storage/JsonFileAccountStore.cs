using System.Text.Json;
using InvestmentServer.Domain;
using Microsoft.Extensions.Logging;
using System.Threading;
using InvestmentServer.Events;

namespace InvestmentServer.Storage;

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

    public AccountState GetAccountState()
    {
        return GetAccountStateAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public IReadOnlyList<InvestmentHistoryItem> GetInvestmentHistory()
    {
        return GetInvestmentHistoryAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public IReadOnlyList<InvestmentOption> GetInvestmentOptions()
    {
        return _investmentOptions.ToList();
    }

    public void SetCurrentUser(string userName)
    {
        _currentUserName = userName;
    }

    public void ClearCurrentUser()
    {
        _currentUserName = null;
    }

    public async Task<InvestResult<ActiveInvestment>> TryStartInvestment(string optionId)
    {
        _logger.LogInformation("TryStartInvestment called on thread {ThreadId}", Thread.CurrentThread.ManagedThreadId);

        if (_currentUserName is null)
            return InvestResult<ActiveInvestment>.Failure("NOT_LOGGED_IN", "User is not logged in");

        var option = _investmentOptions.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            return InvestResult<ActiveInvestment>.Failure("INVALID_OPTION", "Invalid investment option");

        await _gate.WaitAsync();
        try
        {
            var db = await ReadDbUnsafeAsync();

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

            await WriteDbUnsafeAsync(db);

            return InvestResult<ActiveInvestment>.Success(investment);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task CompleteInvestment(string activeInvestmentId, CancellationToken ct = default)
    {
        await CompleteInvestmentAsync(activeInvestmentId, ct);
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

    // --------- async internals for sync members ---------

    private async Task<AccountState> GetAccountStateAsync(CancellationToken ct)
    {
        if (_currentUserName is null)
        {
            // Return default state for not logged in user
            return new AccountState(null, 1000m, new List<ActiveInvestment>(), new List<InvestmentHistoryItem>());
        }

        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            var user = GetOrCreateUserUnsafe(db, _currentUserName);

            // Ensure persisted if newly created
            await WriteDbUnsafeAsync(db, ct);

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

    private async Task<IReadOnlyList<InvestmentHistoryItem>> GetInvestmentHistoryAsync(CancellationToken ct)
    {
        if (_currentUserName is null)
            return new List<InvestmentHistoryItem>();

        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            var user = GetOrCreateUserUnsafe(db, _currentUserName);

            // Ensure persisted if newly created
            await WriteDbUnsafeAsync(db, ct);

            return user.InvestmentHistory
                .OrderByDescending(i => i.CompletedAtUtc)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<long?> CompleteInvestmentAsync(string activeInvestmentId, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var db = await ReadDbUnsafeAsync(ct);
            var now = DateTime.UtcNow;

            // Search across all users for the investment.
            foreach (var user in db.Users.Values)
            {
                var index = user.ActiveInvestments.FindIndex(i => i.Id == activeInvestmentId);
                if (index < 0)
                    continue;

                var investment = user.ActiveInvestments[index];

                // Not due yet
                if (investment.EndTimeUtc > now)
                    return null;

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

                var ev = _eventsHub.Publish(investment.Id, now);
                return ev.Token;
            }
            // If no investment was found and completed, return null
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    // --------- DB model + helpers ---------

    private sealed class AccountsDb
    {
        public Dictionary<string, UserAccount> Users { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class UserAccount
    {
        public string UserName { get; set; } = "";
        public decimal Balance { get; set; } = 1000m;
        public List<ActiveInvestment> ActiveInvestments { get; set; } = new();
        public List<InvestmentHistoryItem> InvestmentHistory { get; set; } = new();
    }

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

    // --------- File IO (caller must hold _gate) ---------

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
}
