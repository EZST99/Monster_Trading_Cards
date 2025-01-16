using System.Data;
using Npgsql;
using SwenProject_Arslan.Models;

namespace SwenProject_Arslan.Handlers.DbHandlers;

using System.Threading.Tasks;

public class UserDbHandler : BaseDbHandler
{
    private readonly string _connectionString;

    public UserDbHandler(string connectionString)
    {
        _connectionString = connectionString;
        if (_connection == null)
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.OpenAsync().GetAwaiter().GetResult(); // Verbindung direkt öffnen
        }
    }
    public static async Task InsertUserAsync(string userName, string passwordhash)
    {
        var query = "INSERT INTO \"user\" (username, passwordhash, coins, elo) VALUES (@username, @passwordhash, @coins, @elo)";
        
        await using var cmd = new NpgsqlCommand(query, _connection, _transaction);
        cmd.Parameters.AddWithValue("username", userName);
        cmd.Parameters.AddWithValue("passwordhash", passwordhash);
        cmd.Parameters.AddWithValue("coins", 29);
        cmd.Parameters.AddWithValue("elo", 100);

        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<User> GetUserByUsernameAsync(string username)
    {
        var query = "SELECT * FROM  \"user\" WHERE username = @username";
        
        await using var cmd = new NpgsqlCommand(query, _connection, _transaction);
        cmd.Parameters.AddWithValue("username", username);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                UserName = reader.GetString("username"),
                PasswordHash = reader.GetString("passwordhash"),
                Coins = reader.GetInt32("coins"),
                ELO = reader.GetInt32("elo")
            };
        }

        return null;
    }

    public static async Task UpdateUserAsync(string username, Dictionary<string, object> updates)
    {
        if (updates == null || updates.Count == 0)
            throw new ArgumentException("No updates provided.");

        var setClauses = string.Join(", ", updates.Keys.Select(key => $"{key} = @{key}"));
        var query = $"UPDATE \"user\" SET {setClauses} WHERE username = @username";

        await using var cmd = new NpgsqlCommand(query, _connection, _transaction);
        cmd.Parameters.AddWithValue("username", username);

        foreach (var kvp in updates)
        {
            cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
        }

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
            throw new InvalidOperationException("User not found or no changes applied.");
    }



}
