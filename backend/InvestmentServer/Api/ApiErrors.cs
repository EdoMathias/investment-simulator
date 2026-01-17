namespace InvestmentServer.Api;

public static class ApiErrors
{
    public static IResult BadRequest(string code, string message)
        => Results.BadRequest(new { code, message });

    public static IResult NotFound(string code, string message)
        => Results.NotFound(new { code, message });
}