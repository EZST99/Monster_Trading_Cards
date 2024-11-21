using System;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using FHTW.Swen1.Swamp.Exceptions;
using MyApp.Models;
using SwenProject_Arslan.Models;


namespace FHTW.Swen1.Swamp
{
    /// <summary>This class represents a user.</summary>
    public sealed class User
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Currently holds the system users.</summary>
        /// <remarks>Is to be removed by database implementation later.</remarks>
        private static Dictionary<string, User> _Users = new();



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Creates a new instance of this class.</summary>
        public User()
        {}



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the user name.</summary>
        public string UserName { get; private set; }
        public string PasswordHash { get;  private set; }
        public int Coins { get; private set; }
        public List<ICard> Stack { get; private set; }
        public List<ICard> Deck { get; private set; }

        
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Saves changes to the user object.</summary>
        /// <param name="token">Token of the session trying to modify the object.</param>
        /// <exception cref="SecurityException">Thrown in case of an unauthorized attempt to modify data.</exception>
        /// <exception cref="AuthenticationException">Thrown when the token is invalid.</exception>
        public void Save(string token)
        {
            (bool Success, User? User) auth = Token.Authenticate(token);
            if(auth.Success)
            {
                if(auth.User!.UserName != UserName)
                {
                    throw new SecurityException("Trying to change other user's data.");
                }
                // Save data.
            }
            else { new AuthenticationException("Not authenticated."); }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public static methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Creates a user.</summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="fullName">Full name.</param>
        /// <param name="eMail">E-mail addresss.</param>
        /// <exception cref="UserException">Thrown when the user name already exists.</exception>
        public static bool Create(string userName, string password)
        {
            if(_Users.ContainsKey(userName))
            {
                return false;
            }

            User user = new()
            {
                UserName = userName,
                PasswordHash = HashPassword(password),
                Coins = 20,
                Stack = new List<ICard>(),
                Deck = new List<ICard>()
            };

            _Users.Add(user.UserName, user);
            return true;
        }


        /// <summary>Performs a user logon.</summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <returns>Returns a tuple of success flag and token.
        ///          If successful, the success flag is TRUE and the token contains a token string,
        ///          otherwise success flag is FALSE and token is empty.</returns>
        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if(_Users.ContainsKey(userName))
            {
                if(VerifyPassword((password), _Users[userName].PasswordHash))
                    return (true, Token._CreateTokenFor(_Users[userName]));
                return (false, string.Empty);
            }

            return (false, string.Empty);
        }
        
        bool AddToStack(ICard card)
        {
            if (card != null)
            {
                Stack.Add(card);
                return true;
            }
        
            return false;
        }

        bool AddToDeck(ICard card)
        {
            if (card != null && Stack.Count > 0 && Deck.Count < 4)
            {
                Deck.Add(card);
                return true;
            }
        
            return false;
        }

        bool RemoveFromStack(ICard card)
        {
            if (Stack.Contains(card))
            {
                Stack.Remove(card);
                return true;
            }
        
            return false;
        }

        bool RemoveFromDeck(ICard card)
        {
            if (Deck.Contains(card))
            {
                Deck.Remove(card);
                return true;
            }
        
            return false;
        }

        bool BuyPackage()
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

        void SelectCardsForDeck()
        {
            var cards = Stack
                .OrderByDescending(card => card.Damage)
                .Take(4)
                .ToList();
        }
    }
    
    
}
