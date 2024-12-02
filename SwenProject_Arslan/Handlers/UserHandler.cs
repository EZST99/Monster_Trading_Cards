using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FHTW.Swen1.Swamp.Exceptions;
using SwenProject_Arslan.Models;

namespace FHTW.Swen1.Swamp
{
    public class UserHandler: Handler, IHandler
    { 
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

                        var isRegistered = User.Create(username, password); // Benutzer erstellen

                        if (isRegistered)
                        {
                            e.Reply(HttpStatusCode.OK, $"User {username} registered successfully.");
                            return true;
                        }
                        else
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "User already exists");
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

                e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
                return true;
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
