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

        // Standardkonstruktor für Produktion
        public UserDbHandler() : this("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg") { }

        // Konstruktor mit ConnectionString für Tests
        public UserDbHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<bool> CheckIfUserExist(User user)
        {
            const string query = "SELECT COUNT(*) FROM \"user\" WHERE username = @username;";
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", user.UserName); 

            try
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new UserException($"Error checking if user exists: {ex.Message}");
            }
        }

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

        public async Task UpdateUserAsync(string userName, string? name, string? bio, string? image, int? coins, int? elo)
        {
            var updateClauses = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            if (name != null)
            {
                updateClauses.Add("Name = @Name");
                parameters.Add(new NpgsqlParameter("@Name", name));
            }

            if (bio != null)
            {
                updateClauses.Add("Bio = @Bio");
                parameters.Add(new NpgsqlParameter("@Bio", bio));
            }

            if (image != null)
            {
                updateClauses.Add("Image = @Image");
                parameters.Add(new NpgsqlParameter("@Image", image));
            }

            if (coins.HasValue)
            {
                updateClauses.Add("Coins = @Coins");
                parameters.Add(new NpgsqlParameter("@Coins", coins.Value));
            }

            if (elo.HasValue)
            {
                updateClauses.Add("Elo = @Elo");
                parameters.Add(new NpgsqlParameter("@Elo", elo.Value));
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
        
        public async Task<User> GetUserByUserNameAsync(string userName) 
        {
            const string query = @"
        SELECT UserName, PasswordHash, Coins, ELO, Name, Bio, Image 
        FROM ""user"" 
        WHERE UserName = @UserName;";

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
                        ELO = reader.GetInt32(3),
                        Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Bio = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Image = reader.IsDBNull(6) ? null : reader.GetString(6)
                    };
                }

                throw new UserException("User not found.");
            }
            catch (Exception ex)
            {
                throw new UserException($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<List<Card>> GetStackFromUser(string userName)
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

        public async Task<List<Card>> GetDeckFromUser(string userName)
        {
            const string stackQuery = "SELECT CardId FROM \"deck\" WHERE UserName = @UserName;";

            List<string> cardIds = new List<string>();
            await using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                await using (var deckCommand = new NpgsqlCommand(stackQuery, connection))
                {
                    deckCommand.Parameters.AddWithValue("@UserName", userName);

                    await using var reader = await deckCommand.ExecuteReaderAsync();
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