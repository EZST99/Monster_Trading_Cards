using Npgsql;
using NpgsqlTypes;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess;

public class CardDbHandler
{
    private readonly string _connectionString;

    public CardDbHandler()
    {
        _connectionString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
    }

    public async Task CreateCardAsync(Card card)
    {
        const string query = "INSERT INTO card (id, packageId, name, damage, elementType, isMonster, monsterType) VALUES (@id, @packageId, @name, @damage, @elementType, @isMonster, @MonsterType)";
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("id", card.Id);
        command.Parameters.AddWithValue("@packageId", card.PackageId);
        command.Parameters.AddWithValue("@name", card.Name);
        command.Parameters.AddWithValue("@damage", card.Damage);
        command.Parameters.AddWithValue("@elementType", card.ElementType.ToString());
        command.Parameters.AddWithValue("@isMonster", card.IsMonster);
        if (card.IsMonster)
        {
            command.Parameters.AddWithValue("@monsterType", card.MonsterType.ToString());
        }
        else
        {
            command.Parameters.AddWithValue("@monsterType", DBNull.Value);
        }

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating card: {ex.Message}");

        }
    }
}