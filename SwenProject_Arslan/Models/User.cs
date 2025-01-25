using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text.Json;
using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Exceptions;
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
        public static List<Card> Stack { get; private set; } = new List<Card>();
        //public List<ICard> Deck { get; private set; }

        public User()
        {}

        public User(string userName, string password)
        {
            UserName = userName;
            PasswordHash = HashPassword(password);
            Coins = 20;
            ELO = 100;
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
        
        public async Task Save(string userName, string? password, int? coins, int? elo)
        {
            UserDbHandler userDbHandler = new UserDbHandler();
            await userDbHandler.UpdateUserAsync(userName, password, coins, elo);
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
                UserDbHandler userDbHandler = new();
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
            UserDbHandler userDbHandler = new();
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
            
            PackageDbHandler packageDbHandler = new();
            var packageId = await packageDbHandler.SelectLatestPackageAsync();
            if (packageId == null)
            {
                throw new InvalidOperationException("No unopened packages available.");
            }
            await AddCardsToStack(user, packageId);
            await packageDbHandler.UpdatePackageIsOpenedAsync(packageId);
            user.Coins -= 5;
            await user.Save(userName, null, user.Coins, null);
        }

        private static async Task AddCardsToStack(User user, int packageId)
        {
            CardDbHandler cardDbHandler = new();
            var cards = await cardDbHandler.GetCardsFromPackage(packageId);
            if (cards == null || !cards.Any())
            {
                throw new InvalidOperationException($"No cards found for package {packageId}.");
            }
            Stack.AddRange(cards);
            StackDbHandler stackDbHandler = new();
            await stackDbHandler.AddCardsToStack(user, cards);

        }

        public static async Task<List<Card>> GetUserCards(string userName)
        {
            UserDbHandler userDbHandler = new();
            var cards = await userDbHandler.GetAllCardsFromUser(userName);
            return cards;
        }
        
        /*public bool AddToStack(Card card)
        {
            if (card != null)
            {
                Stack.Add(card);
                return true;
            }
        
            return false;
        }
        /*
        public bool AddToDeck(ICard card)
        {
            if (card != null && Stack.Count > 0 && Deck.Count < 4)
            {
                Deck.Add(card);
                return true;
            }
        
            return false;
        }

        public bool RemoveFromStack(ICard card)
        {
            if (Stack.Contains(card))
            {
                Stack.Remove(card);
                return true;
            }
        
            return false;
        }

        public bool RemoveFromDeck(ICard card)
        {
            if (Deck.Contains(card))
            {
                Deck.Remove(card);
                return true;
            }
        
            return false;
        }

        public bool BuyPackage()
        {
            if (Coins >= 5)
            {
                Coins -= 5;
                var newPackage = PackageGenerator.GeneratePackage();
                for (int i = 0; i < newPackage.Count; i++)
                {
                    AddToStack(newPackage[i]);
                }

                return true;
            }

            return false;
        }

        public void SelectCardsForDeck()
        {
            var cards = Stack
                .OrderByDescending(card => card.Damage)
                .Take(4)
                .ToList();
        }
        
        public ICard PlayCard()
        {
            return Deck[RandomNumberGenerator.GetInt32(0, Deck.Count - 1)];
        }*/
    }
    
    
}
