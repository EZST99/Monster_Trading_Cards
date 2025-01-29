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
    }
}
