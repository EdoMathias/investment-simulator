// Shared types for the API according to our backend contracts

export type ApiError = {
    code: string;
    message: string;
};

export type LoginRequest = {
    userName: string;
};

export type InvestmentResponse = {
    message: string;
};

export type InvestmentOption = {
    id: string;
    name: string;
    requiredAmount: number;
    expectedReturn: number;
    durationSeconds: number;
};

export type ActiveInvestment = {
    id: string;
    optionId: string;
    name: string;
    investedAmount: number;
    expectedReturn: number;
    startTimeUtc: string;
    endTimeUtc: string;
};

export type InvestmentHistoryItem = {
    id: string;
    optionId: string;
    name: string;
    investedAmount: number;
    returnedAmount: number;
    startTimeUtc: string;
    endTimeUtc: string;
    completedAtUtc: string;
};

export type AccountState = {
    userName: string | null;
    balance: number;
    activeInvestments: ActiveInvestment[];
};

export type InvestmentCompletedEvent = {
    Token: number;
    UserName: string;
    InvestmentId: string;
    OptionId: string;
    Name: string;
    InvestedAmount: number;
    ReturnedAmount: number;
    BalanceAfter: number;
    StartTimeUtc: string;
    EndTimeUtc: string;
    CompletedAtUtc: string;
};