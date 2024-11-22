using System;



namespace FHTW.Swen1.Swamp
{
    /// <summary>This class provides methods for the token-based security.</summary>
    public static class Token
    {
        private static string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        internal static Dictionary<string, User> _Tokens = new();
        
        internal static string _CreateTokenFor(User user)
        {
            string rval = string.Empty;
            Random rnd = new();

            for(int i = 0; i < 24; i++)
            {
                rval += _ALPHABET[rnd.Next(0, 62)];
            }

            _Tokens.Add(rval, user);

            return rval;
        }

        public static (bool Success, User? User) Authenticate(string token)
        {
            if(_Tokens.ContainsKey(token))
            {
                return (true, _Tokens[token]);
            }

            return (false, null);
        }
    }
}
