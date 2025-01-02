using System.Text.Json;
using SwenProject_Arslan.Exceptions;
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
                return HandleUserRequest(e).GetAwaiter().GetResult();
            }

            if (e.Path.StartsWith("/sessions", StringComparison.OrdinalIgnoreCase))
            {
                return HandleSessionRequest(e).GetAwaiter().GetResult();
            }

            return false;
        }

        private static async Task<bool> HandleUserRequest(HttpSvrEventArgs e)
        {
            try
            {
                if (e.Method == "POST")
                {
                    Console.WriteLine($"Received Payload: {e.Payload}");

                    var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);
                    if (requestData == null || !requestData.ContainsKey("Username") || !requestData.ContainsKey("Password"))
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

                    await User.Create(username, password);
                    e.Reply(HttpStatusCode.OK, "User created successfully.");
                    return true;
                }

                e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
                return true;
            }
            catch (UserException ex)
            {
                e.Reply(HttpStatusCode.USER_ALREADY_EXISTS, ex.Message);
                return true;
            }
            catch (Exception ex)
            {
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                Console.WriteLine($"Unhandled exception: {ex}");
                return true;
            }
        }

        private async Task<bool>  HandleSessionRequest(HttpSvrEventArgs e)
        {
            try
            {
                if (e.Method == "POST")
                {
                    try
                    {
                        Console.WriteLine($"Received Payload: {e.Payload}");

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

                        // Warten auf das Ergebnis der Logon-Methode
                        var logonResult = await User.Logon(username, password);

                        if (logonResult.Success)
                        {
                            e.Reply(HttpStatusCode.OK, $"User {username} logged in successfully. Token: {logonResult.Token}");
                        }
                        else
                        {
                            e.Reply(HttpStatusCode.BAD_REQUEST, "Username or password is incorrect.");
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                        Console.WriteLine($"Unhandled exception: {ex}");
                        return true;
                    }
                }


                e.Reply(HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
                return true;
            }
            catch (Exception ex)
            {
                e.Reply(HttpStatusCode.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                Console.WriteLine($"Unhandled exception: {ex}");
                return true;
            }
        }
    }
}
