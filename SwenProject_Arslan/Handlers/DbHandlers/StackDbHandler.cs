using Npgsql;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess;

public class StackDbHandler
{
    private readonly string _connectionString;

    public StackDbHandler()
    {
        _connectionString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
    }

    public async Task AddCardsToStack(User user, List<Card> cards)
    {
        const string query = "INSERT INTO stack (userName, cardId) VALUES (@userName, @cardId)";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        try
        {
            foreach (var card in cards)
            {
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@userName", user.UserName);
                command.Parameters.AddWithValue("@cardId", card.Id);

                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding cards to stack: {ex.Message}");
        }
    }
}