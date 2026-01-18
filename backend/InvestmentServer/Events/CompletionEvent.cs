namespace InvestmentServer.Events;

public sealed record CompletionEvent(
    long Token,
    string InvestmentId,
    DateTime CompletedAtUtc
);