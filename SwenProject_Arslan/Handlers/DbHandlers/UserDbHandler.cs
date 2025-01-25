using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using SwenProject_Arslan.Exceptions;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess
{
    /// <summary>
    /// Handles database operations for User.
    /// </summary>
    public class UserDbHandler
    {
        private readonly string _connectionString;

        public UserDbHandler()
        {
            _connectionString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
        }

        /*private static string ConvertToDatabaseColumnName(string propertyName)
        {
            return propertyName.ToLower();
        }*/

        private async Task<bool> CheckIfUserExist(User user)
        {
            const string query = "SELECT COUNT(*) FROM \"user\" WHERE username = @username;";
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            int count;
            try
            {
                var result = await command.ExecuteScalarAsync();
                count = int.Parse(result.ToString());
            }
            catch (Exception ex)
            {
                throw new UserException($"Error checking if user exists: {ex.Message}");
            }
            return count > 0;
        }
        /// <summary>
        /// Creates a new user and inserts it into the database.
        /// </summary>
        public async Task CreateUserAsync(User user)
        {
            if (await CheckIfUserExist(user))
            {
                throw new UserException($"User already exists: {user.UserName}");
            }
            const string query = "INSERT INTO \"user\" (UserName, PasswordHash, Coins, ELO) VALUES (@UserName, @PasswordHash, @Coins, @ELO);";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Coins", user.Coins);
            command.Parameters.AddWithValue("@ELO", user.ELO);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new UserException($"Error creating user: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates properties of an existing user in the database.
        /// </summary>
        public async Task UpdateUserAsync(string userName, string? password, int? coins, int? elo)
        {
            var updateClauses = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (password != null)
            {
                updateClauses.Add("passwordhash = @password");
                parameters.Add(new NpgsqlParameter("@password", User.HashPassword(password)));
            }

            if (coins.HasValue)
            {
                updateClauses.Add("coins = @coins");
                parameters.Add(new NpgsqlParameter("@coins", coins.Value));
            }

            if (elo.HasValue)
            {
                updateClauses.Add("elo = @elo");
                parameters.Add(new NpgsqlParameter("@elo", elo.Value));
            }

            if (updateClauses.Count == 0)
            {
                throw new ArgumentException("No updates provided.");
            }

            var query = $@"
                UPDATE ""user""
                SET {string.Join(", ", updateClauses)}
                WHERE username = @username;";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("@username", userName));
            command.Parameters.AddRange(parameters.ToArray());

            try
            {
                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new UserException($"User '{userName}' not found.");
                }
            }
            catch (Exception ex)
            {
                throw new UserException($"Error updating user: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        public async Task<User> GetUserByUserNameAsync(string userName)
        {
            const string query = "SELECT UserName, PasswordHash, Coins, ELO FROM \"user\" WHERE UserName = @UserName;";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserName", userName);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        UserName = reader.GetString(0),
                        PasswordHash = reader.GetString(1),
                        Coins = reader.GetInt32(2),
                        ELO = reader.GetInt32(3)
                    };
                }

                throw new UserException("User not found.");
            }
            catch (Exception ex)
            {
                throw new UserException($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<List<Card>> GetAllCardsFromUser(string userName)
        {
            const string stackQuery = "SELECT CardId FROM \"stack\" WHERE UserName = @UserName;";

            List<string> cardIds = new List<string>();
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var stackCommand = new NpgsqlCommand(stackQuery, connection))
                {
                    stackCommand.Parameters.AddWithValue("@UserName", userName);

                    await using var reader = await stackCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        cardIds.Add(reader.GetString(0));
                    }
                }
            }

            if (!cardIds.Any())
            {
                return new List<Card>();
            }

            const string cardQuery = "SELECT id, packageId, name, damage, elementType, isMonster, monsterType FROM card WHERE id = ANY(@CardIds);";
            List<Card> cards = new List<Card>();
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var cardCommand = new NpgsqlCommand(cardQuery, connection))
                {
                    cardCommand.Parameters.AddWithValue("@CardIds", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, cardIds.ToArray());

                    await using var cardReader = await cardCommand.ExecuteReaderAsync();
                    while (await cardReader.ReadAsync())
                    {
                        var card = new Card
                        {
                            Id = cardReader.GetString(0),
                            PackageId = cardReader.GetInt32(1),
                            Name = cardReader.GetString(2),
                            Damage = (float)cardReader.GetDouble(3),
                            ElementType = Enum.Parse<ElementType>(cardReader.GetString(4), true),
                            IsMonster = cardReader.GetBoolean(5),
                            MonsterType = cardReader.IsDBNull(6) ? null : Enum.Parse<MonsterType>(cardReader.GetString(6), true)
                        };
                        cards.Add(card);
                    }
                }
            }

            return cards;
        }
    }
}