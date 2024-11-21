using MyApp.Models;

namespace SwenProject_Arslan.Models;

public static class PackageGenerator
{
    public static List<ICard> GeneratePackage()
    {
        List<ICard> cards = new List<ICard>();
        Random random = new Random();
        for (int i = 0; i < 5; i++)
        {
            int randomCardType = random.Next(0, 1);
            ElementType element = (ElementType)random.Next(0, Enum.GetValues(typeof(ElementType)).Length);
            int damage = random.Next(10, 100);
            
            if (randomCardType == 0)
            {
                MonsterType type = (MonsterType)random.Next(0, Enum.GetValues(typeof(MonsterType)).Length);
                string name = $"{element}{type}";
                cards.Add(new MonsterCard(name, damage, element, type));
            }
            else
            {
                string name = $"{element}Spell";
                cards.Add(new SpellCard(name, damage, element));
            }
        }
        return cards;
    }
}