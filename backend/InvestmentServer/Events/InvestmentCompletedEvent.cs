namespace InvestmentServer.Events;

public sealed record InvestmentCompletedEvent(
    long Token,
    string UserName,
    string InvestmentId,
    string OptionId,
    string Name,
    decimal InvestedAmount,
    decimal ReturnedAmount,
    decimal BalanceAfter,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    DateTime CompletedAtUtc
);