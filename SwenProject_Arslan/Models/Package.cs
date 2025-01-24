using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models.Cards;

namespace SwenProject_Arslan.Models;

public class Package
{
    List<Card> Cards;

    public Package()
    {
    }
    
    public static async Task Create(List<Card> cards)
    {
        Package package = new();
        PackageDbHandler packageDbHandler = new();
        CardDbHandler cardDbHandler = new();
        try
        {
            await packageDbHandler.CreatePackageAsync();
            foreach (Card card in cards)
            {
                await cardDbHandler.CreateCardAsync(card);
            }
            Console.WriteLine("Package successfully created!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating package: {ex.Message}");
            throw;
        }
        

        /*try
        {
            // Beginne eine Transaktion für Paket- und Karteninserts
            await DbHandler.BeginTransactionAsync();

            // Füge das Paket ein
            int packageId = await DbHandler.InsertAsync(package, "id");

            // Füge die Karten ein, verknüpft mit der Paket-ID
            foreach (var card in cards)
            {
                card.PackageId = packageId; // Stelle sicher, dass dies in der Card-Klasse existiert
                await DbHandler.InsertAsync(card, "id");
            }

            // Bestätige die Transaktion
            await DbHandler.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await DbHandler.RollbackTransactionAsync();
            Console.WriteLine($"Error: {ex.Message}");
        }*/
    }

}