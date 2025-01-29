using SwenProject_Arslan.DataAccess;

namespace SwenProject_Arslan.Handlers.DbHandlers;

public static class DbHandlerFactory
{
    private static string _connectionString;
    public static void Initialize(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static UserDbHandler GetUserDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new UserDbHandler(_connectionString);
    }
    
    public static PackageDbHandler GetPackageDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new PackageDbHandler(_connectionString);
    }


    public static CardDbHandler GetCardDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new CardDbHandler(_connectionString);
    }
    
 
    public static DeckDbHandler GetDeckDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new DeckDbHandler(_connectionString);
    }
    

    public static StackDbHandler GetStackDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new StackDbHandler(_connectionString);
    }
}
