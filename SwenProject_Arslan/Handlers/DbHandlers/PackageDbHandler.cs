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
    }
}