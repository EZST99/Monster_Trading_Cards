namespace SwenProject_Arslan.Entities;

public class CardEntity
{
    public int Id { get; set; } // Primärschlüssel
    public string Name { get; set; }
    public int Damage { get; set; }
    public string ElementType { get; set; }
    public string CardType { get; set; } // "Monster" oder "Spell"
    public string? MonsterType { get; set; } // Nur für MonsterCard
}