using SwenProject_Arslan.Entities;
using SwenProject_Arslan.Interfaces;
using SwenProject_Arslan.Models;

namespace SwenProject_Arslan.Helpers;

public static class MappingHelper
{
    public static ICard MapToCard(CardEntity entity)
    {
        if (entity.CardType == "Monster")
        {
            return new MonsterCard(
                entity.Name,
                entity.Damage,
                Enum.Parse<ElementType>(entity.ElementType),
                Enum.Parse<MonsterType>(entity.MonsterType)
            );
        }
        else if (entity.CardType == "Spell")
        {
            return new SpellCard(
                entity.Name,
                entity.Damage,
                Enum.Parse<ElementType>(entity.ElementType)
            );
        }

        throw new InvalidOperationException($"Unknown CardType: {entity.CardType}");
    }

    public static CardEntity MapToEntity(ICard card)
    {
        if (card is MonsterCard monsterCard)
        {
            return new CardEntity
            {
                Name = monsterCard.Name,
                Damage = monsterCard.Damage,
                ElementType = monsterCard.ElementType.ToString(),
                CardType = "Monster",
                MonsterType = monsterCard.MonsterType.ToString()
            };
        }
        else if (card is SpellCard spellCard)
        {
            return new CardEntity
            {
                Name = spellCard.Name,
                Damage = spellCard.Damage,
                ElementType = spellCard.ElementType.ToString(),
                CardType = "Spell"
            };
        }

        throw new InvalidOperationException("Unknown card type");
    }
}