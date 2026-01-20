using System.Collections.Concurrent;
using System.Threading.Channels;

namespace InvestmentServer.Events;

/// <summary>
/// Hub for managing investment completion events
/// </summary>
public sealed class CompletionEventsHub
{
    private long _token;
    private readonly ConcurrentDictionary<Guid, Channel<InvestmentCompletedEvent>> _subs = new();

    public (ChannelReader<InvestmentCompletedEvent> Reader, Action Unsubscribe) Subscribe()
    {
        var id = Guid.NewGuid();
        var ch = Channel.CreateUnbounded<InvestmentCompletedEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _subs[id] = ch;

        void Unsubscribe()
        {
            if (_subs.TryRemove(id, out var removed))
                removed.Writer.TryComplete();
        }

        return (ch.Reader, Unsubscribe);
    }

    public InvestmentCompletedEvent Publish(InvestmentCompletedEvent ev)
    {
        // Assign a unique token
        ev = ev with { Token = Interlocked.Increment(ref _token) };

        foreach (var ch in _subs.Values)
            ch.Writer.TryWrite(ev);

        return ev;
    }
}