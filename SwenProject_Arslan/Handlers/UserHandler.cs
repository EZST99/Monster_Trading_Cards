using System.Text.Json;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Repositories;
using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Handlers
{
    public class UserHandler: Handler, IHandler
    { 
        private readonly UserService _userService;

        public UserHandler() : this(new UserService(new DbHandler("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg")))
        {
        }
        public UserHandler(UserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        public override bool Handle(HttpSvrEventArgs e)
        { 
            if (e.Path.StartsWith("/users", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Method == "POST")
                {
                    try
                    {
                        Console.WriteLine($"Received Payload: {e.Payload}");

                        // JSON-Daten deserialisieren
                        var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                        if (requestData == null ||
                            !requestData.ContainsKey("Username") ||
                            !requestData.ContainsKey("Password"))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST,
                                "Invalid payload format. Expected JSON with 'Username' and 'Password'.");
                            return true;
                        }

                        var username = requestData["Username"].Trim();
                        var password = requestData["Password"].Trim();

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Username and password cannot be empty.");
                            return true;
                        }
                        User.Create(username, password);

                        e.Reply(HttpStatusCode.OK, "User created successfully.");
                        return true; // Erfolgreiche Verarbeitung
                    }
                    catch (Exception ex)
                    {
                        e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                        Console.WriteLine($"Unhandled exception: {ex}");
                        return true;
                    }
                }
                else
                {
                    // Wenn die Methode NICHT POST ist
                    e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
                    return true;
                }
            }
            
            if (e.Path.StartsWith("/sessions", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Method == "POST")
                {
                    try
                    {
                        Console.WriteLine($"Received Payload: {e.Payload}");

                        // JSON-Daten deserialisieren
                        var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);

                        if (requestData == null || 
                            !requestData.ContainsKey("Username") || 
                            !requestData.ContainsKey("Password"))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid payload format. Expected JSON with 'Username' and 'Password'.");
                            return true;
                        }

                        var username = requestData["Username"].Trim();
                        var password = requestData["Password"].Trim();

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Username and password cannot be empty.");
                            return true;
                        }

                        var isLoggedIn = User.Logon(username, password); // Benutzer einloggen
                        if (isLoggedIn != (false, ""))
                        {
                            e.Reply(HttpStatusCode.OK, $"User {username} loged in successfully. Token: {isLoggedIn}");
                            return true;
                        }
                        else
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Username or password is incorrect.");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                        Console.WriteLine($"Unhandled exception: {ex}"); // Fehler für Debugging loggen
                        return true;
                    }
                }

                e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use GET.");
                return true;
            }

            return false;
        }

    }
}
