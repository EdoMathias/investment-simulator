namespace InvestmentServer.Api;

public static class ApiResults
{
    public static IResult Ok<T>(T data)
        => Results.Ok(data);
}