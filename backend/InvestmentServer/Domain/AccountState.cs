namespace InvestmentServer.Domain;

public record AccountState(
    string? UserName,
    decimal Balance,
    IReadOnlyList<ActiveInvestment> ActiveInvestments,
    IReadOnlyList<InvestmentHistoryItem> InvestmentHistory
);