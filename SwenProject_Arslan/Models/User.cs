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
        private User()
        {}



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the user name.</summary>
        public string UserName
        {
            get; private set;
        } = string.Empty;


        /// <summary>Gets or sets the user's full name.</summary>
        public int Coins { get; set; }
        public int ELO { get; set; }
        public List<ICard> Stack { get; private set; }
        public List<ICard> Deck { get; private set; }



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
        
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
        public static void Create(string userName, string password)
        {
            if(_Users.ContainsKey(userName))
            {
                throw new UserException("User name already exists.");
            }

            User user = new()
            {
                UserName = userName,
                Coins = 20,
                ELO = 100,
                Stack = new List<ICard>(),
                Deck = new List<ICard>()
            };

            _Users.Add(user.UserName, user);
        }


        /// <summary>Gets a user by user name.</summary>
        /// <param name="userName">User name.</param>
        /// <returns>Return a user object if the user was found, otherwise returns NULL.</returns>
        public static User? Get(string userName) 
        {
            _Users.TryGetValue(userName, out User? user);
            return user;
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
                return (true, Token._CreateTokenFor(_Users[userName]));
            }

            return (false, string.Empty);
        }
        
        public bool AddToStack(ICard card)
        {
            if (card != null)
            {
                Stack.Add(card);
                return true;
            }
        
            return false;
        }

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
        }
    }
    
    
}
