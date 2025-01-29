using Npgsql;
using NUnit.Framework;
using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Handlers.DbHandlers;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Models.Cards;

namespace Unittests_Arslan
{
    [TestFixture]
    public class PackageTests
    {
        [SetUp]
        public async Task Setup()
        {
            // Initialisiert die Factory mit der Test-Datenbank
            DbHandlerFactory.Initialize("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");

            // Bereinigt die Test-Datenbank
            await using var connection = new NpgsqlConnection("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");
            await connection.OpenAsync();

            await using var cleanPackagesCmd = new NpgsqlCommand("DELETE FROM \"package\";", connection);
            await cleanPackagesCmd.ExecuteNonQueryAsync();

            await using var cleanCardsCmd = new NpgsqlCommand("DELETE FROM \"card\";", connection);
            await cleanCardsCmd.ExecuteNonQueryAsync();
        }

        [Test]
        public async Task CreatePackage_WithValidCards_Success()
        {
            List<Card> cards = new()
            {
                new Card { Id = "card1", Name = "FireDragon", Damage = 50, ElementType = ElementType.Fire, IsMonster = true, MonsterType = MonsterType.Dragon },
                new Card { Id = "card2", Name = "WaterSpell", Damage = 30, ElementType = ElementType.Water, IsMonster = false },
                new Card { Id = "card3", Name = "Wizard", Damage = 40, ElementType = ElementType.Normal, IsMonster = true, MonsterType = MonsterType.Wizard },
                new Card { Id = "card4", Name = "FireElf", Damage = 20, ElementType = ElementType.Fire, IsMonster = true, MonsterType = MonsterType.FireElf },
                new Card { Id = "card5", Name = "WaterGoblin", Damage = 25, ElementType = ElementType.Water, IsMonster = true, MonsterType = MonsterType.Goblin }
            };

            await Package.Create(cards);

            await using var connection = new NpgsqlConnection("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");
            await connection.OpenAsync();

            const string packageQuery = "SELECT COUNT(*) FROM \"package\";";
            await using var packageCmd = new NpgsqlCommand(packageQuery, connection);
            var packageCount = await packageCmd.ExecuteScalarAsync();
            Assert.That(packageCount, Is.EqualTo(1));

            const string cardQuery = "SELECT COUNT(*) FROM \"card\" WHERE packageId IS NOT NULL;";
            await using var cardCmd = new NpgsqlCommand(cardQuery, connection);
            var cardCount = await cardCmd.ExecuteScalarAsync();
            Assert.That(cardCount, Is.EqualTo(5));
        }

        [Test]
        public async Task CreatePackage_WithEmptyList_ThrowsException()
        {
            // Arrange
            List<Card> cards = new(); // Leere Liste

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await Package.Create(cards);
            });

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Does.Contain("No cards were provided"));
        }

        [Test]
        public async Task CreatePackage_CardsAssignedCorrectPackageId()
        {
            // Arrange
            List<Card> cards = new()
            {
                new Card { Id = "cardA", Name = "WaterKraken", Damage = 60, ElementType = ElementType.Water, IsMonster = true, MonsterType = MonsterType.Kraken },
                new Card { Id = "cardB", Name = "FireOrk", Damage = 45, ElementType = ElementType.Fire, IsMonster = true, MonsterType = MonsterType.Ork }
            };

            // Act
            await Package.Create(cards);

            await using var connection = new NpgsqlConnection("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");
            await connection.OpenAsync();

            const string packageIdQuery = "SELECT DISTINCT packageId FROM \"card\";";
            await using var packageIdCmd = new NpgsqlCommand(packageIdQuery, connection);
            await using var reader = await packageIdCmd.ExecuteReaderAsync();

            HashSet<int> uniquePackageIds = new();
            while (await reader.ReadAsync())
            {
                uniquePackageIds.Add(reader.GetInt32(0));
            }
            
            // Assert
            Assert.That(uniquePackageIds.Count, Is.EqualTo(1));
        }
    }
}
