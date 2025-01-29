using Npgsql;
using SwenProject_Arslan.Handlers.DbHandlers;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace Unittests_Arslan;

[TestFixture]
public class CardTests
{
    [SetUp]
    public async Task Setup()
    {
        
    }

    [Test]
    public void GetDamage_WaterspellAttacksKnight_ReturnsMaxValue()
    {
        Card attacker = new()
        {
            ElementType = ElementType.Water,
            IsMonster = false,
            Damage = 20
        };

        Card attacked = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Knight,
            Damage = 30
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(float.MaxValue));
    }

    [Test]
    public void GetDamage_SpellCardAttacksKraken_ReturnsZero()
    {
        Card attacker = new()
        {
            IsMonster = false,
            ElementType = ElementType.Fire,
            Damage = 30
        };
        
        Card attacked = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Kraken,
            Damage = 50
        };
        
        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(0));
    }

    [Test]
    public void GetDamage_GoblinAttacksDragon_ReturnsZero()
    {
        Card attacker = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Goblin,
            Damage = 20
        };

        Card attacked = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Dragon,
            Damage = 50
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(0));
    }

    [Test]
    public void GetDamage_OrkAttacksWizard_ReturnsZero()
    {
        Card attacker = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Ork,
            Damage = 40
        };

        Card attacked = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Wizard,
            Damage = 30
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(0));
    }

    [Test]
    public void GetDamage_DragonAttacksFireElf_ReturnsZero()
    {
        Card attacker = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.Dragon,
            Damage = 50
        };
        
        Card attacked = new()
        {
            IsMonster = true,
            MonsterType = MonsterType.FireElf,
            Damage = 30
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(0));
    }

    [Test]
    public void GetDamage_FireSpellAgainstNormalMonster_DoubleDamage()
    {
        Card attacker = new()
        {
            IsMonster = false,
            ElementType = ElementType.Fire,
            Damage = 25
        };

        Card attacked = new()
        {
            IsMonster = true,
            ElementType = ElementType.Normal,
            Damage = 40
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(50)); // 25 * 2 = 50
    }

    [Test]
    public void GetDamage_WaterSpellAgainstFireMonster_DoubleDamage()
    {
        Card attacker = new()
        {
            IsMonster = false,
            ElementType = ElementType.Water,
            Damage = 30
        };

        Card attacked = new()
        {
            IsMonster = true,
            ElementType = ElementType.Fire,
            Damage = 50
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(60)); // 30 * 2 = 60
    }

    [Test]
    public void GetDamage_FireMonsterAgainstWaterSpell_HalfDamage()
    {
        Card attacker = new()
        {
            IsMonster = true,
            ElementType = ElementType.Fire,
            Damage = 40
        };

        Card attacked = new()
        {
            IsMonster = false,
            ElementType = ElementType.Water,
            Damage = 20
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(20)); // 40 / 2 = 20
    }

    [Test]
    public void GetDamage_NormalMonsterAgainstWaterSpell_DoubleDamage()
    {
        Card attacker = new()
        {
            IsMonster = true,
            ElementType = ElementType.Normal,
            Damage = 40
        };

        Card attacked = new()
        {
            IsMonster = false,
            ElementType = ElementType.Water,
            Damage = 20
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(80)); // 40 * 2 = 80
    }

    [Test]
    public void GetDamage_MonsterVsMonster_ReturnsBaseDamage()
    {
        Card attacker = new()
        {
            IsMonster = true,
            ElementType = ElementType.Fire,
            Damage = 35
        };

        Card attacked = new()
        {
            IsMonster = true,
            ElementType = ElementType.Fire,
            Damage = 50
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(35)); 
    }

    [Test]
    public void GetDamage_SameElementFight_ReturnsBaseDamage()
    {
        Card attacker = new()
        {
            IsMonster = false,
            ElementType = ElementType.Water,
            Damage = 30
        };

        Card attacked = new()
        {
            IsMonster = false,
            ElementType = ElementType.Water,
            Damage = 20
        };

        float damage = attacker.GetDamage(attacked);
        
        Assert.That(damage, Is.EqualTo(30)); 
    }
}
