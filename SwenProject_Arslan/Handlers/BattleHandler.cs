using System.Text.Json.Nodes;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Handlers
{
    public class BattleHandler : Handler, IHandler
    {
        private static (HttpSvrEventArgs? EventArgs, string? PlayerName)? _waitingPlayer;

        public override bool Handle(HttpSvrEventArgs e)
        {
            return false;
        }

        public static async Task<bool> HandleBattleRequest(HttpSvrEventArgs e1, HttpSvrEventArgs e2)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;
            try
            {
                var authorizationHeader1 = e1.Headers
                    .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(authorizationHeader1) || !authorizationHeader1.StartsWith("Bearer "))
                {
                    e1.Reply(SwenProject_Arslan.Server.HttpStatusCodes.UNAUTHORIZED,
                        "Authorization header is missing or invalid.");
                    return true;
                }

                var token1 = authorizationHeader1.Substring("Bearer ".Length).Trim();

                var (isAuthenticated1, authenticatedUser1) = Token.Authenticate(token1);
                if (!isAuthenticated1)
                {
                    e1.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                    return true;
                }

                var authorizationHeader2 = e2.Headers
                    .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                if (string.IsNullOrEmpty(authorizationHeader2) || !authorizationHeader2.StartsWith("Bearer "))
                {
                    e2.Reply(SwenProject_Arslan.Server.HttpStatusCodes.UNAUTHORIZED,
                        "Authorization header is missing or invalid.");
                    return true;
                }

                var token2 = authorizationHeader2.Substring("Bearer ".Length).Trim();

                var (isAuthenticated2, authenticatedUser2) = Token.Authenticate(token2);
                if (!isAuthenticated2)
                {
                    e1.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                    return true;
                }

                Battle battle = new();
                string[] battleLogs = await battle.StartBattle(authenticatedUser1.UserName, authenticatedUser2.UserName);

                reply = new JsonObject()
                {
                    ["success"] = true,
                    ["message"] = "Battle has finished.",
                    ["battle_log"] = new JsonArray(battleLogs.Select(log => JsonValue.Create(log)).ToArray())
                };
                status = HttpStatusCodes.OK;

            }
            catch (Exception e)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = e.Message ?? "Error handling request." };
            }
            
            e1.Reply(status, reply.ToJsonString());
            e2.Reply(status, reply.ToJsonString());
            
            return true;
        }

        /*public static async Task<bool> HandleBattleRequest(HttpSvrEventArgs e1, HttpSvrEventArgs e2)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;
            try
            {
                Console.WriteLine($"Processing battle request");

                if (e.Method == "POST")
                {
                    // Autorisierungslogik
                    var authorizationHeader = e.Headers
                        .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

                    if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Authorization header is missing or invalid.");
                        return true;
                    }

                    var token = authorizationHeader.Substring("Bearer ".Length).Trim();
                    var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
                    if (!isAuthenticated || authenticatedUser == null)
                    {
                        e.Reply(HttpStatusCodes.UNAUTHORIZED, "Invalid or expired token.");
                        return true;
                    }

                    string currentPlayerName = authenticatedUser.UserName;
                    Console.WriteLine($"Player {currentPlayerName} is authenticated.");

                    lock (_lock)
                    {
                        if (_waitingPlayer == null)
                        {
                            _waitingPlayer = (e, currentPlayerName);
                            e.Reply(HttpStatusCodes.OK, "Waiting for an opponent...");
                            Console.WriteLine($"Player {currentPlayerName} is waiting for an opponent.");
                            return true;
                        }
                    }

                    var (firstEventArgs, firstPlayerName) = _waitingPlayer.Value;
                    _waitingPlayer = null;

                    Console.WriteLine($"Starting battle between {firstPlayerName} and {currentPlayerName}.");

                    // Battle starten
                    var battle = new Battle();
                    var result = await battle.StartBattle(firstPlayerName, currentPlayerName);

                    firstEventArgs.Reply(HttpStatusCodes.OK, result);
                    e.Reply(HttpStatusCodes.OK, result);

                    return true;
                }

                e.Reply(HttpStatusCodes.BAD_REQUEST, "Method not allowed. Use POST.");
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"Client already disposed during battle request: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                e.Reply(HttpStatusCodes.INTERNAL_SERVER_ERROR, $"An unexpected error occurred: {ex.Message}");
                Console.WriteLine($"Error in BattleHandler: {ex}");
                return true;
            }
        }
*/
    }
}
