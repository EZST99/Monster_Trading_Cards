using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using SwenProject_Arslan.Exceptions;
using SwenProject_Arslan.Models;

namespace SwenProject_Arslan.DataAccess
{
    /// <summary>
    /// Handles database operations for User entities.
    /// </summary>
    public class UserDbHandler
    {
        private readonly string _connectionString;

        public UserDbHandler()
        {
            _connectionString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
        }

        /// <summary>
        /// Creates a new user and inserts it into the database.
        /// </summary>

        private static string ConvertToDatabaseColumnName(string propertyName)
        {
            return propertyName.ToLower();
        }

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
        public async Task UpdateUserAsync(string token, string userName, Dictionary<string, object> updates)
        {
            // Authenticate
            var auth = Token.Authenticate(token);
            if (!auth.Success || auth.User!.UserName != userName)
            {
                throw new SecurityException("Unauthorized update attempt.");
            }

            // Prepare updates
            var updateQueries = new List<string>();
            var parameters = new Dictionary<string, object> { { "@username", userName } };

            int index = 0;
            foreach (var update in updates)
            {
                // Validate property
                if (!typeof(User).GetProperties().Any(p => p.Name.Equals(update.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException($"Invalid property name: {update.Key}");
                }

                string dbColumn = ConvertToDatabaseColumnName(update.Key);
                updateQueries.Add($"{dbColumn} = @param{index}");

                // Konvertiere den Wert in den erwarteten Typ
                var targetType = typeof(User).GetProperty(update.Key)?.PropertyType;
                var value = update.Value is JsonElement jsonElement
                    ? jsonElement.Deserialize(targetType)
                    : Convert.ChangeType(update.Value, targetType);

                parameters[$"@param{index}"] = value;
                index++;
            }

            var query = $"UPDATE \"user\" SET {string.Join(", ", updateQueries)} WHERE username = @username;";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(query, connection);
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }

            try
            {
                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new UserException("Update failed: No rows affected.");
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
    }
}