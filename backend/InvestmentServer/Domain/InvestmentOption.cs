namespace InvestmentServer.Domain;

public record InvestmentOption(
    string Id,
    string Name,
    decimal RequiredAmount,
    decimal ExpectedReturn,
    int DurationSeconds
);