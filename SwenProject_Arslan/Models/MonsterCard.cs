using SwenProject_Arslan.Interfaces;

namespace SwenProject_Arslan.Models;

public class MonsterCard : ICard
{
    public string Name { get; set; }
    public int Damage { get; set; }
    public ElementType ElementType { get; set; }
    public MonsterType MonsterType { get; set; }

    public MonsterCard(string name, int damage, ElementType elementType, MonsterType monsterType)
    {
        Name = name;
        Damage = damage;
        ElementType = elementType;
        MonsterType = monsterType;
    }
    
    public int getDamage(ICard otherCard)
    {
        return Damage;
    }
}