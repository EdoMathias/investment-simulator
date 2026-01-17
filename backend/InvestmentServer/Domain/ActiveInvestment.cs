namespace InvestmentServer.Domain;

public record ActiveInvestment(
    string Id,
    string OptionId,
    string Name,
    decimal InvestedAmount,
    decimal ExpectedReturn,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc
);