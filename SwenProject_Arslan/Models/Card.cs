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
    
    public float GetDamage(Card otherCard)
    {
        if (!IsMonster && ElementType == ElementType.Water && otherCard.IsMonster && otherCard.MonsterType == Models.MonsterType.Knight)
        {
            return float.MaxValue; // Sofortiger Tod des Ritters
        }

        if (otherCard.IsMonster && otherCard.MonsterType == Models.MonsterType.Kraken && !IsMonster)
        {
            return 0;
        }
        if (!(IsMonster && otherCard.IsMonster))
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
            }
        }

        if (MonsterType == Models.MonsterType.Goblin && otherCard.MonsterType == Models.MonsterType.Dragon)
        {
            return 0;
        }

        if (MonsterType == Models.MonsterType.Ork && otherCard.MonsterType == Models.MonsterType.Wizard)
        {
            return 0;
        }

        if (MonsterType == Models.MonsterType.Dragon && otherCard.MonsterType == Models.MonsterType.FireElf)
        {
            return 0;
        }
        
        return Damage;
    }
}