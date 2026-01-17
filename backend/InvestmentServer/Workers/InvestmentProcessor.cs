using InvestmentServer.Storage;

namespace InvestmentServer.Workers;

/// <summary>
/// A background worker that will track the active investments and complete them when they are due
/// </summary>
public sealed class InvestmentProcessor : BackgroundService
{
    private readonly IAccountStore _accountStore;

    // Constructor
    public InvestmentProcessor(IAccountStore accountStore)
    {
        _accountStore = accountStore;
    }

    // Start the background service
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Track all active investments and complete them when they are due
        while (!stoppingToken.IsCancellationRequested)
        {
            // Get a copy of the active investments and convert to list so we can modify it
            var activeInvestments = _accountStore.GetAccountState().ActiveInvestments.ToList();
            
            var now = DateTime.UtcNow;

            // Complete each active investment that is due
            foreach (var investment in activeInvestments)
            {
                if (investment.EndTimeUtc <= now)
                {
                    // Complete the investment
                    Console.WriteLine($"Investment {investment.Id} ({investment.Name}) is due. Completing it...");
                    _accountStore.CompleteInvestment(investment.Id);
                    Console.WriteLine($"Investment {investment.Id} ({investment.Name}) completed successfully");
                }
            }

            // Waiting for 250ms to ensure we have "near-real-time" experience
            await Task.Delay(250, stoppingToken);
        }
    }
}