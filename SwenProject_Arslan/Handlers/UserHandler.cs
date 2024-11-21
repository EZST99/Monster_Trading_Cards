using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FHTW.Swen1.Swamp.Exceptions;
using SwenProject_Arslan.Models;

namespace FHTW.Swen1.Swamp
{
    public class UserHandler: Handler, IHandler
    { 
        public override bool Handle(HttpSvrEventArgs e)
        {
            if (e.Path.StartsWith("/register", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Method == "POST")
                {
                    try
                    {
                        Console.WriteLine($"Received Payload: {e.Payload}");

                        var userData = e.Payload.Split('&');
                        if (userData.Length < 2 || !userData[0].StartsWith("username=") || !userData[1].StartsWith("password="))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid payload format. Expected 'username=...&password=...'.");
                            return true;
                        }
                        var username = userData[0].Replace("username=", "").Trim();
                        var password = userData[1].Replace("password=", "").Trim();

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
            
            if (e.Path.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
            {
                if (e.Method == "GET")
                {
                    try
                    {
                        Console.WriteLine($"Received Payload: {e.Payload}");

                        var userData = e.Payload.Split('&');
                        if (userData.Length < 2 || !userData[0].StartsWith("username=") || !userData[1].StartsWith("password="))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Invalid payload format. Expected 'username=...&password=...'.");
                            return true;
                        }
                        var username = userData[0].Replace("username=", "").Trim();
                        var password = userData[1].Replace("password=", "").Trim();

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Username and password cannot be empty.");
                            return true;
                        }

                        var isLoggedIn = User.Logon(username, password); // Benutzer einloggen
                        if (isLoggedIn != (false, ""))
                        {
                            e.Reply(HttpStatusCode.OK, $"User {username} loged in successfully.");
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
