namespace SwenProject_Arslan.Handlers;
using Npgsql;
using System;

public class TestHandler
{
    public static async void connectDb()
    {
        var connString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        // Insert some data
        //await using (var cmd = new NpgsqlCommand("INSERT INTO \"user\" (id, username, password, fullname, email) VALUES (@id, @u, @pw, @f, @em)", conn))
        //{
        //    cmd.Parameters.AddWithValue("id", 3);
        //    cmd.Parameters.AddWithValue("u", "test");
        //    cmd.Parameters.AddWithValue("pw", "11111111");
        //    cmd.Parameters.AddWithValue("f", "test test");
        //    cmd.Parameters.AddWithValue("em", "test@test.at");
        //    await cmd.ExecuteNonQueryAsync();
        //}
        // Retrieve all rows
        await using (var cmd = new NpgsqlCommand("SELECT * FROM \"users\"", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                int id = reader.GetInt32(0);
                string username = reader.GetString(1);
                string password= reader.GetString(2);
                int coins = reader.GetInt32(3);
                int elo = reader.GetInt32(4);
                Console.WriteLine($"ID: {id}, Username: {username}, Password: {password}, Coins: {coins}, ELO: {elo}");
            }
                
        }
    }
}