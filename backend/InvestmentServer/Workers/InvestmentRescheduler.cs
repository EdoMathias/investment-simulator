using InvestmentServer.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvestmentServer.Workers;

/// <summary>
/// Rescheduler for active investments
/// Runs on a background thread and reschedules investments when they are due
/// in case of a restart of the server
/// </summary>
public sealed class InvestmentRescheduler : BackgroundService
{
    private readonly IAccountStore _store;
    private readonly InvestmentCompletionScheduler _scheduler;
    private readonly ILogger<InvestmentRescheduler> _logger;

    public InvestmentRescheduler(IAccountStore store, InvestmentCompletionScheduler scheduler, ILogger<InvestmentRescheduler> logger)
    {
        _store = store;
        _scheduler = scheduler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Rescheduling active investments...");

        var active = await _store.GetAllActiveInvestmentsAsync(stoppingToken);
        foreach (var inv in active)
            _scheduler.Schedule(inv);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
