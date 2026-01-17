using InvestmentServer.Contracts;

namespace InvestmentServer.Api;

public static class ApiErrors
{
    public static IResult BadRequest(string code, string message)
        => Results.BadRequest(new ApiError(code, message));

    public static IResult NotFound(string code, string message)
        => Results.NotFound(new ApiError(code, message));
}