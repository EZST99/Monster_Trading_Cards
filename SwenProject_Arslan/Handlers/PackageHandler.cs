using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;
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

            var authorizationHeader = e.Headers
                .FirstOrDefault(h => h.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.UNAUTHORIZED, "Authorization header is missing or invalid.");
                return true;
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            var (isAuthenticated, authenticatedUser) = Token.Authenticate(token);
            if (!isAuthenticated)
            {
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.UNAUTHORIZED, "Invalid or expired token.");
                return true;
            }

            try
            {
                var cards = JsonSerializer.Deserialize<List<Card>>(e.Payload);

                if (cards == null || cards.Count != 5)
                {
                    throw new ArgumentException("A package must contain exactly 5 cards.");
                }

                foreach (var card in cards)
                {
                    card.ElementType = DetermineElementType(card.Name); 
                    card.IsMonster = IsMonsterCard(card.Name); 
                    card.MonsterType = card.IsMonster ? DetermineMonsterType(card.Name) : null; // Nur für Monster
                }

                await Package.Create(cards);

                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.OK, "Package created successfully.");
            }
            catch (Exception ex)
            {
                e.Reply(SwenProject_Arslan.Server.HttpStatusCode.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
            }

            return true;
        }

        private MonsterType DetermineMonsterType(string cardName)
        {
            foreach (var enumValue in Enum.GetValues(typeof(MonsterType)))
            {
                if(cardName.Contains(enumValue.ToString()))
                    return (MonsterType)enumValue;
            }
            throw new ArgumentException($"Card name {cardName} is invalid. Must contain valid monster type.");
        }

        private bool IsMonsterCard(string cardName)
        {
            foreach (var enumValue in Enum.GetValues(typeof(MonsterType)))
            {
                if(cardName.Contains(enumValue.ToString()))
                    return true;
            }
            return false;
        }

        private ElementType DetermineElementType(string cardName)
        {
            foreach (var enumValue in Enum.GetValues(typeof(ElementType)))
            {
                if(cardName.Contains(enumValue.ToString()))
                    return (ElementType)enumValue;
            }

            return ElementType.Normal;
        }
    }
}