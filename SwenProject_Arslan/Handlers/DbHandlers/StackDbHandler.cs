using Npgsql;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess;

public class StackDbHandler
{
    private readonly string _connectionString;

    // Standardkonstruktor für Produktion
    public StackDbHandler() : this("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg") { }

    // Konstruktor mit ConnectionString für Tests
    public StackDbHandler(string connectionString)
    {
        _connectionString = connectionString;
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