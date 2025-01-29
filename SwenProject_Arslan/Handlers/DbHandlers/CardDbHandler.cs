using Npgsql;
using NpgsqlTypes;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.DataAccess;

public class CardDbHandler
{
    private readonly string _connectionString;

    // Standardkonstruktor für Produktion
    public CardDbHandler() : this("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg") { }

    // Konstruktor mit ConnectionString für Tests
    public CardDbHandler(string connectionString)
    {
        _connectionString = connectionString;
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
        catch (Exception e)
        {
            throw new Exception($"Error creating card: {e.Message}");

        }
    }

    public async Task<List<Card>> GetCardsFromPackage(int packageId)
    {
        const string query = "SELECT id, packageId, name, damage, elementType, isMonster, monsterType FROM card WHERE packageId = @packageId";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("packageId", packageId);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            List<Card> cards = new List<Card>();

            while (reader.Read())
            {
                var card = new Card
                {
                    Id = reader.GetString(0),  // `id` ist VARCHAR
                    PackageId = reader.GetInt32(1),  // `packageId` ist SERIAL/INTEGER
                    Name = reader.GetString(2),  // `name` ist VARCHAR
                    Damage = (float)reader.GetDouble(3),  // `damage` ist FLOAT
                    ElementType = Enum.Parse<ElementType>(reader.GetString(4), true), // `elementType` ist VARCHAR
                    IsMonster = reader.GetBoolean(5),  // `isMonster` ist BOOLEAN
                    MonsterType = reader.IsDBNull(6) ? null : Enum.Parse<MonsterType>(reader.GetString(6), true) // `monsterType` ist VARCHAR oder NULL
                };
                cards.Add(card);
            }
            return cards;
        }
        catch (Exception e)
        {
            throw new Exception($"Error getting cards from package: {e.Message}");
        }
    }
}