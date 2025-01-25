using System;
using System.Threading.Tasks;
using Npgsql;
using SwenProject_Arslan.Models;

namespace SwenProject_Arslan.DataAccess
{
    /// <summary>
    /// Handles database operations for Package entities.
    /// </summary>
    public class PackageDbHandler
    {
        private readonly string _connectionString;

        public PackageDbHandler()
        {
            _connectionString = "Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg";
        }

        /// <summary>
        /// Creates a new package and inserts it into the database.
        /// </summary>
        public async Task<int> CreatePackageAsync()
        {
            int packageId;
            const string query = "INSERT INTO Package DEFAULT VALUES RETURNING Id;";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                await using var command = new NpgsqlCommand(query, connection);
                packageId = (int)await command.ExecuteScalarAsync();
                Console.WriteLine($"Package created with ID: {packageId}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating package: {ex.Message}");
            }
            return packageId;
        }

        public async Task<int> SelectLatestPackageAsync()
        {
            const string query = "SELECT id FROM Package WHERE isOpened = false ORDER BY id DESC LIMIT 1;";
            
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            await using var command = new NpgsqlCommand(query, connection);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return reader.GetInt32(0);
                }
                throw new Exception($"No package found");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error selecting latest package: {ex.Message}");
            }
        }
        
        public async Task UpdatePackageIsOpenedAsync(int packageId)
        {
            const string query = "UPDATE Package SET isOpened = @isOpened WHERE id = @id;";
    
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
    
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", packageId);
            command.Parameters.AddWithValue("@isOpened", true); 
            try
            {
                int rowsAffected = await command.ExecuteNonQueryAsync(); 
                if (rowsAffected == 0)
                {
                    throw new Exception($"No rows were updated for package ID {packageId}. Make sure the ID exists.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating package: {e.Message}");
                throw;
            }
        }

    }
}