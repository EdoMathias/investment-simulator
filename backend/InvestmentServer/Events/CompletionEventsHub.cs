using System.Collections.Concurrent;
using System.Threading.Channels;

namespace InvestmentServer.Events;

public sealed class CompletionEventsHub
{
    private long _token;
    private readonly ConcurrentDictionary<Guid, Channel<CompletionEvent>> _subs = new();

    public (ChannelReader<CompletionEvent> Reader, Action Unsubscribe) Subscribe()
    {
        var id = Guid.NewGuid();
        var ch = Channel.CreateUnbounded<CompletionEvent>(new UnboundedChannelOptions
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

    public CompletionEvent Publish(string investmentId, DateTime completedAtUtc)
    {
        var ev = new CompletionEvent(
            Token: Interlocked.Increment(ref _token),
            InvestmentId: investmentId,
            CompletedAtUtc: completedAtUtc
        );

        foreach (var ch in _subs.Values)
            ch.Writer.TryWrite(ev);

        return ev;
    }
}