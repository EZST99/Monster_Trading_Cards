using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.Json;
using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Exceptions;
using SwenProject_Arslan.Handlers.DbHandlers;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.Models
{
    /// <summary>This class represents a user.</summary>
    public sealed class User
    {
        private static Dictionary<string, User> _Users = new();
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public int Coins { get; set; }
        public int ELO { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
        public static List<Card> Stack { get; private set; } = new List<Card>();
        public static List<Card> Deck { get; private set; } = new List<Card>();

        public User()
        {}

        public User(string userName, string password)
        {
            UserName = userName;
            PasswordHash = HashPassword(password);
            Coins = 20;
            ELO = 100;
            Name = null;
            Bio = null;
            Image = null;
        }
        
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(32);

            return computedHash.SequenceEqual(hash);
        }
        
        public async Task Save(string userName, string? name, string? bio, string? image, int? elo, int? coins)
        {
            var userDbHandler = DbHandlerFactory.GetUserDbHandler();
            await userDbHandler.UpdateUserAsync(userName, name, bio, image, coins, elo);
        }

        
        public static async Task Create(string userName, string password)
        {
            User user = new()
            {
                UserName = userName,
                PasswordHash = HashPassword(password),
                Coins = 20,
                ELO = 100,
                //Stack = new List<ICard>(),
                //Deck = new List<ICard>()
            };
            try
            {
                //await DbHandler.InsertAsync(user, "UserName");
                var userDbHandler = DbHandlerFactory.GetUserDbHandler();
                await userDbHandler.CreateUserAsync(user);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw new UserException($"Error: {ex.Message}");
            }
        }

        public static async Task<User> GetUserByUserName(string userName)
        {
            //return await DbHandler.GetByColumnAsync<User>("Username", userName);
            var userDbHandler = DbHandlerFactory.GetUserDbHandler();
            return await userDbHandler.GetUserByUserNameAsync(userName);
        }

        public static async Task<(bool Success, string Token)> Logon(string userName, string password)
        {
            var userByUsername = await GetUserByUserName(userName);
            if (userByUsername != null)
            {
                if (VerifyPassword(password, userByUsername.PasswordHash))
                    return (true, Token._CreateTokenFor(userByUsername));
            }

            return (false, string.Empty);
        }

        public static async Task BuyPackage(string userName)
        {
            var user = await GetUserByUserName(userName);
            if (user == null)
            {
                throw new UserException($"User {userName} not found.");
            }

            if (user.Coins < 5)
            {
                throw new UserException($"User {userName} does not have enough coins");
            }
            
            var packageDbHandler = DbHandlerFactory.GetPackageDbHandler();
            var packageId = await packageDbHandler.SelectPackageAsync();
            if (packageId == null)
            {
                throw new InvalidOperationException("No unopened packages available.");
            }
            await AddCardsToStack(user, packageId);
            await packageDbHandler.UpdatePackageIsOpenedAsync(packageId);
            user.Coins -= 5;
            await user.Save(userName, null, null, null, null, user.Coins);
        }

        private static async Task AddCardsToStack(User user, int packageId)
        {
            var cardDbHandler = DbHandlerFactory.GetCardDbHandler();
            var cards = await cardDbHandler.GetCardsFromPackage(packageId);
            if (cards == null || !cards.Any())
            {
                throw new InvalidOperationException($"No cards found for package {packageId}.");
            }
            Stack.AddRange(cards);
            var stackDbHandler = DbHandlerFactory.GetStackDbHandler();
            await stackDbHandler.AddCardsToStack(user, cards);

        }

        private static async Task<bool> CheckIfCardsInStack(User user, List<String> deckCardIds)
        {
            var stack =  await GetUserStack(user.UserName);
            var stackCardIds = stack.Select(card => card.Id).ToList();
            foreach (var deckCardId in deckCardIds)
            {
                if (!stackCardIds.Contains(deckCardId))
                {
                    return false;
                }
                
            }

            return true;
        }

        public static async Task AddCardsToDeck(User user, List<String> cardIds)
        {
            await CheckIfCardsInStack(user, cardIds);
            var deckDbHandler = DbHandlerFactory.GetDeckDbHandler();
            await deckDbHandler.InsertIntoDeck(user, cardIds);
        }

        public static async Task<List<Card>> GetUserStack(string userName)
        {
            var userDbHandler = DbHandlerFactory.GetUserDbHandler();
            var stack = await userDbHandler.GetStackFromUser(userName);
            return stack;
        }
        
        public static async Task<List<Card>> GetUserDeck(string userName)
        {
            var userDbHandler = DbHandlerFactory.GetUserDbHandler();
            var deck = await userDbHandler.GetDeckFromUser(userName);
            return deck;
        }
    }
    
    
}
