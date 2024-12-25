using System.Text.Json;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Handlers;

public class PackageHandler : Handler, IHandler
{
    public override bool Handle(HttpSvrEventArgs e)
    {
        if (e.Path.StartsWith("/package", StringComparison.OrdinalIgnoreCase))
        {
            if (e.Method == "GET")
            {
                Console.WriteLine($"Received Payload: {e.Payload}");
                
                // JSON-Daten deserialisieren
                var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                if (requestData == null || 
                    !requestData.ContainsKey("UserToken"))
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid payload format. Expected JSON with 'UserToken'.");
                    return true;
                }

                var userToken = requestData["UserToken"].Trim();
                
                if (string.IsNullOrEmpty(userToken))
                {
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Token cannot be empty.");
                    return true;
                }
                else
                {
                    (bool Success, User? User) auth = Token.Authenticate(userToken);
                    var (success, user) = Token.Authenticate(userToken);

                    if (!success)
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid or expired token.");
                        return true;
                    }

                    // Paket kaufen
                   /* if (!user.BuyPackage())
                    {
                        e.Reply(HttpStatusCode.BAD_REQUEST, "Not enough coins to buy a package.");
                        return true;
                    }*/

                    e.Reply(HttpStatusCode.OK, $"User {user.UserName} purchased package successfully.");
                    return true;
                }
                
                
            }
        }

        return false;
    }
}