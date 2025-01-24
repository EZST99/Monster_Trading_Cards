namespace SwenProject_Arslan.Models.Cards;

public class Card
{
    public string Id { get; set; }
    public int PackageId { get; set; }
    public string Name { get; set; }
    public float Damage { get; set; }
    public ElementType ElementType { get; set; }
    public bool IsMonster { get; set; }
    public MonsterType? MonsterType { get; set; }
    
    double getDamage(Card otherCard)
    {
        if(!IsMonster)
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
        }
        return Damage;
    }
}