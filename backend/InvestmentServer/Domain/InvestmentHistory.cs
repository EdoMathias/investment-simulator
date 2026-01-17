namespace InvestmentServer.Domain;

public record InvestmentHistoryItem(
    string Id,
    string OptionId,
    string Name,
    decimal InvestedAmount,
    decimal ReturnedAmount,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    DateTime CompletedAtUtc
);