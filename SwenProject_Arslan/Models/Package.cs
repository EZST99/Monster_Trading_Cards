using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Handlers.DbHandlers;
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
        var packageDbHandler = DbHandlerFactory.GetPackageDbHandler();
        var cardDbHandler = DbHandlerFactory.GetCardDbHandler();
        if (cards == null || cards.Count == 0)
        {
            throw new ArgumentException("No cards were provided");
        }
        try
        {
            int packageId = await packageDbHandler.CreatePackageAsync();

            foreach (var card in cards)
            {
                card.PackageId = packageId;
                await cardDbHandler.CreateCardAsync(card);
            }

            Console.WriteLine("Package successfully created!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating package: {ex.Message}");
            throw;
        }
    }

}