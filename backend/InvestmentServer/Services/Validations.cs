using System.Text.RegularExpressions;

namespace InvestmentServer.Utils;

// Helper methods for validations
public static class Validations
{

    // Validate user name for login
    public static bool IsValidUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        // Limit the user name length to 3-20 characters
        return _userNameRegex.IsMatch(userName);
    }

    private static readonly Regex _userNameRegex = new("^[A-Za-z]{3,20}$", RegexOptions.Compiled);
}