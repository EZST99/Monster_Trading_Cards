using System.Text.Json;
using SwenProject_Arslan.Entities;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Handlers;

public class PackageHandler : Handler, IHandler
{
    private readonly DbHandler _dbHandler;

    public PackageHandler() : this(new DbHandler("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg"))
    {
    }
    public PackageHandler(DbHandler dbHandler)
    {
        _dbHandler = dbHandler ?? throw new ArgumentNullException(nameof(dbHandler));
    }
    public override bool Handle(HttpSvrEventArgs e)
    {
        if (e.Path.StartsWith("/package", StringComparison.OrdinalIgnoreCase))
        {
            return HandlePackageRequestTest(e);
        }

        if (e.Path.StartsWith("/packages", StringComparison.OrdinalIgnoreCase))
        {
            return HandleCreatePackage(e).GetAwaiter().GetResult();
        }

        return false;
    }

    public async Task<bool> HandleCreatePackage(HttpSvrEventArgs e)
    {
        if (e.Method != "POST")
        {
            e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
            return true;
        }

        // 1. Authorization-Header überprüfen
        var authorizationHeader = e.Headers
            .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer admin-mtcgToken"))
        {
            e.Reply(HttpStatusCode.UNAUTHORIZED, "Unauthorized access.");
            return true;
        }

        // 2. Payload auslesen und deserialisieren
        List<ICard>? cards;
        try
        {
            cards = JsonSerializer.Deserialize<List<ICard>>(e.Payload);
        }
        catch
        {
            e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid JSON format.");
            return true;
        }

        // Validierung: Genau 5 Karten erforderlich
        if (cards == null || cards.Count != 5)
        {
            e.Reply(HttpStatusCode.BAD_REQUEST, "A package must contain exactly 5 cards.");
            return true;
        }

        try
        {
            // 3. Neues Package erstellen
            //var package = new Package(cards);




            // 5. Erfolgsantwort senden
            e.Reply(HttpStatusCode.OK, $"Package created successfully.");
        }
        catch (Exception ex)
        {
            e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
        }

        return true;
    }


    private bool HandlePackageRequestTest(HttpSvrEventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
            Console.WriteLine($"Unhandled exception: {ex}");
            return true;
        }

        return true;
    }
}