using Npgsql;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess;

public class DeckDbHandler
{
    private readonly string _connectionString;

    public DeckDbHandler()
    {
        _connectionString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
    }
    
    public async Task InsertIntoDeck(User user, List<String> cardIds)
    {
        const string deleteQuery = "DELETE FROM deck WHERE userName=@userName";
        const string insterQuery = "INSERT INTO deck (userName, cardId) VALUES (@userName, @cardId)";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        try
        {
            await using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection))
            {
                deleteCommand.Parameters.AddWithValue("@userName", user.UserName);
                await deleteCommand.ExecuteNonQueryAsync();
            }
            foreach (var card in cardIds)
            {
                await using var insertCommand = new NpgsqlCommand(insterQuery, connection);
                insertCommand.Parameters.AddWithValue("@userName", user.UserName);
                insertCommand.Parameters.AddWithValue("@cardId", card);

                await insertCommand.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding cards to stack: {ex.Message}");
        }
    }
}