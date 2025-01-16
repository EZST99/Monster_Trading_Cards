using SwenProject_Arslan.Interfaces;

namespace SwenProject_Arslan.Models;

public class Package
{
    List<ICard> Cards;

    public Package()
    {
    }
    
    public static async Task Create(List<ICard> cards)
    {
        Package package = new()
        {
            Cards = cards
        };

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