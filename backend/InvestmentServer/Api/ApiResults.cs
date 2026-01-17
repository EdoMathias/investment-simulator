namespace InvestmentServer.Api;

public static class ApiResults
{
    public static IResult OkData<T>(T data)
        => Results.Ok(data);
}