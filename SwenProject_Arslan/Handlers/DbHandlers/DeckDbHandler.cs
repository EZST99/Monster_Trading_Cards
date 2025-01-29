using Npgsql;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess;

public class DeckDbHandler
{
    private readonly string _connectionString;

    // Standardkonstruktor für Produktion
    public DeckDbHandler() : this("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg") { }

    // Konstruktor mit ConnectionString für Tests
    public DeckDbHandler(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task InsertIntoDeck(User user, List<string> cardIds)
    {
        const string checkQuery = "SELECT COUNT(*) FROM stack WHERE userName=@userName AND cardId=@cardId";
        const string deleteQuery = "DELETE FROM deck WHERE userName=@userName";
        const string insertQuery = "INSERT INTO deck (userName, cardId) VALUES (@userName, @cardId)";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var transaction = await connection.BeginTransactionAsync(); 

        try
        {
            foreach (var card in cardIds)
            {
                await using var checkCommand = new NpgsqlCommand(checkQuery, connection, transaction);
                checkCommand.Parameters.AddWithValue("@userName", user.UserName);
                checkCommand.Parameters.AddWithValue("@cardId", card);

                var count = (long)await checkCommand.ExecuteScalarAsync(); 

                if (count == 0)
                {
                    throw new Exception($"Error: Card {card} is not in the user's stack and cannot be added to the deck.");
                }
            }

            await using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection, transaction))
            {
                deleteCommand.Parameters.AddWithValue("@userName", user.UserName);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            foreach (var card in cardIds)
            {
                await using var insertCommand = new NpgsqlCommand(insertQuery, connection, transaction);
                insertCommand.Parameters.AddWithValue("@userName", user.UserName);
                insertCommand.Parameters.AddWithValue("@cardId", card);
                await insertCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync(); 
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(); 
            throw new Exception($"Error adding cards to deck: {ex.Message}");
        }
    }

}