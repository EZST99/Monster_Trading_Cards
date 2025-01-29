using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SwenProject_Arslan.Exceptions;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;
using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Handlers
{
    public class UserHandler: Handler, IHandler
    { 
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

            if (e.Path.StartsWith("/cards", StringComparison.OrdinalIgnoreCase))
            {
                return HandleCardRequest(e).GetAwaiter().GetResult();
            }

            if (e.Path.StartsWith("/deck", StringComparison.OrdinalIgnoreCase))
            {
                return HandleDeckRequest(e).GetAwaiter().GetResult();
            }

            return false;
        }

        private async Task<bool> HandleUserRequest(HttpSvrEventArgs e)
        {
            try
            {
                if (e.Method == "POST")
                {
                    Console.WriteLine($"Received Payload: {e.Payload}");

                    var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(e.Payload);
                    if (requestData == null || !requestData.ContainsKey("Username") || !requestData.ContainsKey("Password"))
                    {
                        e.Reply(HttpStatusCodes.BAD_REQUEST, "Invalid payload format. Expected JSON with 'Username' and 'Password'.");
                        return true;
                    }

                    var username = requestData["Username"].Trim();
                    var password = requestData["Password"].Trim();

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        e.Reply(HttpStatusCodes.BAD_REQUEST, "Username and password cannot be empty.");
                        return true;
                    }

                    await User.Create(username, password);
                    e.Reply(HttpStatusCodes.OK, "User created successfully.");
                    return true;
                }

                if (e.Method == "GET")
                {
                    var username = e.Path.Split('/').LastOrDefault();
                    var authorizationHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                    if (string.IsNullOrEmpty(authorizationHeader))
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Authorization header is missing.");
                        return true;
                    }

                    var tokenParts = authorizationHeader.Split(' ');
                    if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid authorization format.");
                        return true;
                    }

                    var token = tokenParts[1];

                    // Token validation
                    var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
                    if (!isAuthenticated || authenticatedUser == null)
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                        return true;
                    }

                    if (authenticatedUser.UserName != username)
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Not permitted to access this userdata.");
                    }

                    // Fetch user data
                    var user = await User.GetUserByUserName(username);
                    if (user == null)
                    {
                        e.Reply(HttpStatusCodes.NOT_FOUND, "User not found.");
                        return true;
                    }

                    var userJson = JsonSerializer.Serialize(user);
                    e.Reply(HttpStatusCodes.OK, userJson);
                    return true;
                }
                if (e.Method == "PUT")
                {
                    var username = e.Path.Split('/').LastOrDefault();
                    var authorizationHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                    if (string.IsNullOrEmpty(authorizationHeader))
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Authorization header is missing.");
                        return true;
                    }

                    var tokenParts = authorizationHeader.Split(' ');
                    if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid authorization format.");
                        return true;
                    }

                    var token = tokenParts[1];

                    // Token validation
                    var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
                    if (!isAuthenticated || authenticatedUser == null)
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                        return true;
                    }
                    
                    if (authenticatedUser.UserName != username)
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Not permitted to edit this userdata.");
                    }

                    var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(e.Payload);
                    if (requestData == null || requestData.Count == 0)
                    {
                        e.Reply(HttpStatusCodes.BAD_REQUEST, "Invalid payload format. At least one field must be provided.");
                        return true;
                    }

                    string? name = null;
                    string? bio = null;
                    string? image = null;

                    if (requestData.ContainsKey("Name"))
                    {
                        var nameElement = (JsonElement)requestData["Name"];
                        if (nameElement.ValueKind == JsonValueKind.String)
                        {
                            name = nameElement.GetString();
                        }
                        else
                        {
                            throw new InvalidCastException("Name value is not a valid string.");
                        }
                    }

                    if (requestData.ContainsKey("Bio"))
                    {
                        var bioElement = (JsonElement)requestData["Bio"];
                        if (bioElement.ValueKind == JsonValueKind.String)
                        {
                            bio = bioElement.GetString();
                        }
                        else
                        {
                            throw new InvalidCastException("Bio value is not a valid string.");
                        }
                    }

                    if (requestData.ContainsKey("Image"))
                    {
                        var ImageElement = (JsonElement)requestData["Image"];
                        if (ImageElement.ValueKind == JsonValueKind.String)
                        {
                            image = ImageElement.GetString();
                        }
                        else
                        {
                            throw new InvalidCastException("Image value is not a valid string.");
                        }
                    }

                    try
                    {
                        var user = await User.GetUserByUserName(username);
                        if (user == null)
                        {
                            e.Reply(HttpStatusCodes.NOT_FOUND, "User not found.");
                            return true;
                        }

                        await user.Save(username, name, bio, image, null, null);
                        e.Reply(HttpStatusCodes.OK, "User updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        e.Reply(HttpStatusCodes.BAD_REQUEST, ex.Message);
                    }

                    return true;
                }



                e.Reply(HttpStatusCodes.BAD_REQUEST, "Method not allowed. Use POST, GET or PUT.");
                return true;
            }
            catch (UserException ex)
            {
                e.Reply(HttpStatusCodes.USER_ALREADY_EXISTS, ex.Message);
                return true;
            }
            catch (Exception ex)
            {
                e.Reply(HttpStatusCodes.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
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
                            e.Reply(HttpStatusCodes.BAD_REQUEST, "Invalid payload format. Expected JSON with 'Username' and 'Password'.");
                            return true;
                        }

                        var username = requestData["Username"].Trim();
                        var password = requestData["Password"].Trim();

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            e.Reply(HttpStatusCodes.BAD_REQUEST, "Username and password cannot be empty.");
                            return true;
                        }

                        var logonResult = await User.Logon(username, password);

                        if (logonResult.Success)
                        {
                            e.Reply(HttpStatusCodes.OK, $"User {username} logged in successfully. Token: {logonResult.Token}");
                        }
                        else
                        {
                            e.Reply(HttpStatusCodes.BAD_REQUEST, "Username or password is incorrect.");
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        e.Reply(HttpStatusCodes.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                        Console.WriteLine($"Unhandled exception: {ex}");
                        return true;
                    }
                }
                
                e.Reply(HttpStatusCodes.BAD_REQUEST, "Method not allowed. Use POST.");
                return true;
            }
            catch (Exception ex)
            {
                e.Reply(HttpStatusCodes.INTERNAL_SERVER_ERROR, "An unexpected error occurred.");
                Console.WriteLine($"Unhandled exception: {ex}");
                return true;
            }
        }

        private async Task<bool> HandleCardRequest(HttpSvrEventArgs e)
        {
            if (e.Method == "GET")
            {
                var authorizationHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Authorization header is missing.");
                    return true;
                }

                var tokenParts = authorizationHeader.Split(' ');
                if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid authorization format.");
                    return true;
                }

                var token = tokenParts[1];

                // Token validation
                var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
                if (!isAuthenticated || authenticatedUser == null)
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                    return true;
                }
                var stack = await User.GetUserStack(authenticatedUser.UserName);
                var cardsJson = JsonSerializer.Serialize(stack);
                e.Reply(HttpStatusCodes.OK, cardsJson);
                return true;
            }
            
            e.Reply(HttpStatusCodes.BAD_REQUEST, "Method not allowed. Use GET.");
            return true;
        }

        private async Task<bool> HandleDeckRequest(HttpSvrEventArgs e)
        {
            if (e.Method == "GET")
            {
                var authorizationHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Authorization header is missing.");
                    return true;
                }

                var tokenParts = authorizationHeader.Split(' ');
                if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid authorization format.");
                    return true;
                }

                var token = tokenParts[1];

                // Token validation
                var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
                if (!isAuthenticated || authenticatedUser == null)
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                    return true;
                }

                try
                {
                    var deck = await User.GetUserDeck(authenticatedUser.UserName);

                    if (deck == null || !deck.Any())
                    {
                        e.Reply(HttpStatusCodes.OK, "[]");
                        return true;
                    }

                    // Prüfen, ob "format=plain" gesetzt ist
                    var isPlainFormat = e.QueryParameters.TryGetValue("format", out var format) && format.Equals("plain", StringComparison.OrdinalIgnoreCase);

                    if (isPlainFormat)
                    {
                        // Deck in Plain-Text-Format umwandeln
                        var plainTextDeck = string.Join(Environment.NewLine, deck.Select((card, index) => $"Card {index + 1}: {card.Id} - {card.Name} - {card.Damage} Damage"));
                        e.Reply(HttpStatusCodes.OK, plainTextDeck);
                    }
                    else
                    {
                        var cardsJson = JsonSerializer.Serialize(deck);
                        e.Reply(HttpStatusCodes.OK, cardsJson);
                    }
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCodes.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
                }

                return true;
            }


            if (e.Method == "PUT")
            {
                var authorizationHeader = e.Headers.FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Authorization header is missing.");
                    return true;
                }

                var tokenParts = authorizationHeader.Split(' ');
                if (tokenParts.Length != 2 || !tokenParts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid authorization format.");
                    return true;
                }

                var token = tokenParts[1];

                // Token validation
                var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
                if (!isAuthenticated || authenticatedUser == null)
                {
                    e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                    return true;
                }
                try
                {
                    var cards = JsonSerializer.Deserialize<List<String>>(e.Payload);

                    if (cards == null || cards.Count != 4)
                    {
                        throw new ArgumentException("The deck must contain exactly 4 cards.");
                    }
                    
                    await User.AddCardsToDeck(authenticatedUser, cards);
                    
                    e.Reply(HttpStatusCodes.OK, "Deck created successfully.");
                }
                catch (Exception ex)
                {
                    e.Reply(HttpStatusCodes.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
                }

                return true;
            }
            
            e.Reply(HttpStatusCodes.BAD_REQUEST, "Method not allowed. Use GET or PUT.");
            return true;
        }
    }
}
