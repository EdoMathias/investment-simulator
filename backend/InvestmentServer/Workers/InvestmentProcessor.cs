using InvestmentServer.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvestmentServer.Workers;

/// <summary>
/// A background worker that will track the active investments and complete them when they are due
/// </summary>
public sealed class InvestmentProcessor : BackgroundService
{
    private readonly IAccountStore _accountStore;
    private readonly ILogger<InvestmentProcessor> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="accountStore"></param>
    /// <param name="logger"></param>
    public InvestmentProcessor(IAccountStore accountStore, ILogger<InvestmentProcessor> logger)
    {
        _accountStore = accountStore;
        _logger = logger;
    }

    /// <summary>
    /// Start the background service.
    /// Tracks active investments and completes them when they are due.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Investment Processor started");

        // Track all active investments and complete them when they are due
        while (!stoppingToken.IsCancellationRequested)
        {
            // Get a copy of the active investments so we don't modify the source while iterating
            var activeInvestments = _accountStore.GetAccountState().ActiveInvestments.ToList();

            var now = DateTime.UtcNow;

            // Complete each active investment that is due
            foreach (var investment in activeInvestments)
            {
                if (investment.EndTimeUtc <= now)
                {
                    // Complete the investment
                    _logger.LogInformation($"Investment {investment.Id} ({investment.Name}) is due. Completing it...");
                    _accountStore.CompleteInvestment(investment.Id);
                    _logger.LogInformation($"Investment {investment.Id} ({investment.Name}) completed successfully");
                }
            }

            // Waiting for 250ms to ensure we have "near-real-time" experience
            await Task.Delay(250, stoppingToken);
        }
    }
}