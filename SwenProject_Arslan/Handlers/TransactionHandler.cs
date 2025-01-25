using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Handlers;

public class TransactionHandler : Handler, IHandler
{
    public override bool Handle(HttpSvrEventArgs e)
    {
        if (e.Path.StartsWith("/transactions/packages", StringComparison.InvariantCultureIgnoreCase))
        {
            return HandleBuyPackage(e).GetAwaiter().GetResult();
        }

        return false;
    }

    public async Task<bool> HandleBuyPackage(HttpSvrEventArgs e)
    {
        if (e.Method != "POST")
        {
            e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
            return true;
        }

        var authorizationHeader = e.Headers
            .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            e.Reply(HttpStatusCode.UNAUTHORIZED, "Authorization header is missing or invalid.");
            return true;
        }

        var token = authorizationHeader.Substring("Bearer ".Length).Trim();

        var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
        if (!isAuthenticated)
        {
            e.Reply(HttpStatusCode.UNAUTHORIZED, "Invalid or expired token.");
            return true;
        }

        try
        {
            await User.BuyPackage(authenticatedUser.UserName);
            e.Reply(HttpStatusCode.OK, "Package bought successfully.");
        }
        catch (Exception ex)
        {
            e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
        }

        return true;
    }
}