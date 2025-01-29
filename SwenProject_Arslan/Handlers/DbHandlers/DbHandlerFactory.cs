using SwenProject_Arslan.DataAccess;

namespace SwenProject_Arslan.Handlers.DbHandlers;

public static class DbHandlerFactory
{
    private static string _connectionString;

    /// <summary>
    /// Initialisiert die Factory mit einem Connection-String.
    /// Diese Methode sollte nur einmal aufgerufen werden (z. B. im Setup oder Programmstart).
    /// </summary>
    public static void Initialize(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Gibt eine Instanz von UserDbHandler zurück.
    /// </summary>
    public static UserDbHandler GetUserDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new UserDbHandler(_connectionString);
    }

    /// <summary>
    /// Gibt eine Instanz von PackageDbHandler zurück.
    /// </summary>
    public static PackageDbHandler GetPackageDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new PackageDbHandler(_connectionString);
    }

    /// <summary>
    /// Gibt eine Instanz von CardDbHandler zurück.
    /// </summary>
    public static CardDbHandler GetCardDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new CardDbHandler(_connectionString);
    }
    
    /// <summary>
    /// Gibt eine Instanz von DeckDbHandler zurück.
    /// </summary>
    public static DeckDbHandler GetDeckDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new DeckDbHandler(_connectionString);
    }
    
    /// <summary>
    /// Gibt eine Instanz von StackDbHandler zurück.
    /// </summary>
    public static StackDbHandler GetStackDbHandler()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("DatabaseHandlerFactory wurde nicht initialisiert.");
        }

        return new StackDbHandler(_connectionString);
    }
}
