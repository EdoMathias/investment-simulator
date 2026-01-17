using InvestmentServer.Contracts;

namespace InvestmentServer.Domain;

public sealed record InvestResult<T>(
    bool IsSuccess,
    T? Data,
    ApiError? Error)
{
    public static InvestResult<T> Success(T data) =>
        new(true, data, null);

    public static InvestResult<T> Failure(string code, string message) =>
        new(false, default, new ApiError(code, message));
}