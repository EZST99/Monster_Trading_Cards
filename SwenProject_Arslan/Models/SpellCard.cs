using MyApp.Models;

namespace SwenProject_Arslan.Models;

public class SpellCard : ICard
{
    public string Name { get; set; }
    public int Damage { get; set; }
    public ElementType ElementType { get; set; }

    public SpellCard(string name, int damage, ElementType elementType)
    {
        Name = name;
        Damage = damage;
        ElementType = elementType;
    }
    
    public int getDamage(ICard otherCard)
    {
        switch (ElementType)
        {
            case ElementType.Water:
                if (otherCard.ElementType == ElementType.Fire)
                    return Damage * 2;
                if (otherCard.ElementType == ElementType.Normal)
                    return Damage / 2;
                break;
            case ElementType.Fire:
                if (otherCard.ElementType == ElementType.Normal)
                    return Damage * 2;
                if (otherCard.ElementType == ElementType.Water)
                    return Damage / 2;
                break;
            case ElementType.Normal:
                if (otherCard.ElementType == ElementType.Water)
                    return Damage * 2;
                if (otherCard.ElementType == ElementType.Fire)
                    return Damage / 2;
                break;
            default:
                return Damage;
        }

        return Damage;
    }
}