using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Server;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace SwenProject_Arslan.Handlers
{
    public class PackageHandler : Handler, IHandler
    {
        public override bool Handle(HttpSvrEventArgs e)
        {
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
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.BAD_REQUEST, "Method not allowed. Use POST.");
                return true;
            }

            // Authorization überprüfen
            var authorizationHeader = e.Headers
                .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.UNAUTHORIZED, "Authorization header is missing or invalid.");
                return true;
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            // Validierung des Tokens
            var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
            if (!isAuthenticated)
            {
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.UNAUTHORIZED, "Invalid or expired token.");
                return true;
            }

            try
            {
                // Package erstellen
                await Package.Create();

                // Erfolgsantwort senden
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.OK, "Package created successfully.");
            }
            catch (Exception ex)
            {
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
            }

            return true;
        }


    }
}