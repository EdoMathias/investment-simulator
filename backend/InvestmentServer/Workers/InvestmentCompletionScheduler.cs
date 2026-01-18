using InvestmentServer.Domain;
using InvestmentServer.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvestmentServer.Workers;

public sealed class InvestmentCompletionScheduler
{
    private readonly IAccountStore _store;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<InvestmentCompletionScheduler> _logger;

    public InvestmentCompletionScheduler(IAccountStore store, IHostApplicationLifetime lifetime, ILogger<InvestmentCompletionScheduler> logger)
    {
        _store = store;
        _lifetime = lifetime;
        _logger = logger;
    }

    public void Schedule(ActiveInvestment investment)
    {
        var ct = _lifetime.ApplicationStopping;

        _ = Task.Run(async () =>
        {
            try
            {
                var delay = investment.EndTimeUtc - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, ct);

                await _store.CompleteInvestmentAsync(investment.Id, ct);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed completing investment {Id}", investment.Id);
            }
        }, ct);
    }
}
