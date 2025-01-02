using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class DbHandler
{
    private static string _connectionString;

    public DbHandler(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Hilfsmethode für das Setzen von doppelten Anführungszeichen
    private static string QuoteTableName(string tableName)
    {
        var reservedKeywords = new HashSet<string>
        {
            "user", "select", "insert", "update", "delete", "from", "where", "table"
        };

        if (reservedKeywords.Contains(tableName.ToLower()))
        {
            return $"\"{tableName}\"";
        }
        
        return tableName;
    }

    // Create - Einfügen eines neuen Datensatzes
    public static async Task InsertAsync<T>(T entity)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var tableName = typeof(T).Name.ToLower(); // Tabellenname anhand des Klassennamens
        tableName = QuoteTableName(tableName); // Tabellennamen korrekt setzen

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

        await using var cmd = new NpgsqlCommand(query, conn);

        foreach (var property in properties)
        {
            cmd.Parameters.AddWithValue($"@{property.Name}", property.GetValue(entity));
        }

        await cmd.ExecuteNonQueryAsync();
    }

    // Read - Alle Datensätze abfragen
    public async Task<List<T>> GetAllAsync<T>()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var tableName = typeof(T).Name.ToLower();
        tableName = QuoteTableName(tableName); // Tabellennamen korrekt setzen

        var query = $"SELECT * FROM {tableName}";

        var result = new List<T>();

        await using var cmd = new NpgsqlCommand(query, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var entity = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var columnValue = reader[property.Name];
                if (columnValue != DBNull.Value)
                {
                    property.SetValue(entity, columnValue);
                }
            }

            result.Add(entity);
        }

        return result;
    }

    // Read - Einzelnen Datensatz abfragen
    public async Task<T> GetByIdAsync<T>(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var tableName = typeof(T).Name.ToLower();
        tableName = QuoteTableName(tableName); // Tabellennamen korrekt setzen

        var query = $"SELECT * FROM {tableName} WHERE id = @id";

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var entity = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var columnValue = reader[property.Name];
                if (columnValue != DBNull.Value)
                {
                    property.SetValue(entity, columnValue);
                }
            }

            return entity;
        }

        return default;
    }

    // Update - Datensatz aktualisieren
    public async Task UpdateAsync<T>(T entity, int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var tableName = typeof(T).Name.ToLower();
        tableName = QuoteTableName(tableName); // Tabellennamen korrekt setzen

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

        var query = $"UPDATE {tableName} SET {setClause} WHERE id = @id";

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("id", id);

        foreach (var property in properties)
        {
            cmd.Parameters.AddWithValue($"@{property.Name}", property.GetValue(entity));
        }

        await cmd.ExecuteNonQueryAsync();
    }

    // Delete - Datensatz löschen
    public async Task DeleteAsync<T>(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var tableName = typeof(T).Name.ToLower();
        tableName = QuoteTableName(tableName); // Tabellennamen korrekt setzen

        var query = $"DELETE FROM {tableName} WHERE id = @id";

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();
    }
}
