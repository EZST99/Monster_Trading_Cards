using Npgsql;
using System;
using System.Threading.Tasks;
namespace SwenProject_Arslan.Handlers.DbHandlers;

public abstract class BaseDbHandler
{
    protected static NpgsqlConnection _connection;
    protected static NpgsqlTransaction _transaction;

    public static async Task InitializeAsync(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync();
    }

    public static void BeginTransaction()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not initialized. Call InitializeAsync first.");
        
        _transaction = _connection.BeginTransaction();
    }

    public static async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public static async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }
}
