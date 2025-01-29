using Npgsql;
using NpgsqlTypes;
using NUnit.Framework;
using SwenProject_Arslan.Handlers;
using SwenProject_Arslan.DataAccess;
using SwenProject_Arslan.Exceptions;
using SwenProject_Arslan.Handlers.DbHandlers;
using SwenProject_Arslan.Models;

namespace Unittests_Arslan
{
    [TestFixture]
    public class UserTests
    {
        [SetUp]
        public async Task Setup()
        {
            // Initialisiert die Factory mit der Testdatenbank
            DbHandlerFactory.Initialize("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");

            // Bereinigt die Test-Datenbank
            await using var connection = new NpgsqlConnection("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");
            await connection.OpenAsync();

            await using var cleanUsersCmd = new NpgsqlCommand("DELETE FROM \"user\";", connection);
            await cleanUsersCmd.ExecuteNonQueryAsync();
            
            await using var cleanCardsCmd = new NpgsqlCommand("DELETE FROM \"card\";", connection);
            await cleanCardsCmd.ExecuteNonQueryAsync();
            
            await using var cleanPackagesCmd = new NpgsqlCommand("DELETE FROM \"package\";", connection);
            await cleanPackagesCmd.ExecuteNonQueryAsync();
        }


        [Test]
        public async Task Create_UserAlreadyExists_ThrowsException()
        {
            // Arrange
            string username = "Testuser";
            string password = "Testpassword";

            // Act/Assert
            await User.Create(username, password);
                
            var ex = Assert.ThrowsAsync<UserException>(async () =>
            {
                await User.Create(username, password);
            });

            // Assert
            Assert.IsNotNull(ex);
            Assert.AreEqual($"User already exists: {username}", ex.Message);
        }

        [Test]
        public async Task Create_UserDoesNotExist_Success()
        {
            // Arrange
            string username = "Testuser2";
            string password = "Testpassword";

            // Act
            await User.Create(username, password);
            var user = await User.GetUserByUserName(username);

            // Assert
            Assert.That(user, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(user.UserName, Is.EqualTo(username));
                Assert.That(User.VerifyPassword(password, user.PasswordHash), Is.True);
                Assert.That(user.Coins, Is.EqualTo(20));
                Assert.That(user.ELO, Is.EqualTo(100));
            });
        }

        [Test]
        public async Task Logon_UserDoesNotExist_ThrowsException()
        {
            // Arrange
            string username = "Testuser3";
            string password = "Testpassword";
            
            // Act
            var e = Assert.ThrowsAsync<UserException>(async () =>
            { 
                await User.Logon(username, password);
            });
            
            // Assert
            Assert.That(e, Is.Not.Null);
            Assert.That("Error retrieving user: User not found.", Is.EqualTo(e.Message));
        }

        [Test]
        public async Task Logon_UserExists_WrongPassword_ThrowsException()
        {
            // Arrange
            string username = "Testuser4";
            string password = "Testpassword";
            
            // Act 
            await User.Create(username, password);
            var token = await User.Logon(username, "Wrongpassword");
            
            // Assert
            Assert.That(token.Success, Is.False);
            Assert.That(token.Token, Is.Empty);
        }

        [Test]
        public async Task Logon_UserExists_Success()
        {
            // Arrange
            string username = "Testuser5";
            string password = "Testpassword";
            
            // Act
            await User.Create(username, password);
            var token = await User.Logon(username, password);
            
            // Assert
            Assert.That(token.Success, Is.True);
        }

        [Test]
        public async Task Save_NoUpdatesProvided_ThrowsException()
        {
            // Arrange
            string username = "Testuser6";
            string password = "Testpassword";
            
            // Act
            await User.Create(username, password);
            var userBeforeSave = await User.GetUserByUserName(username);
            
            // Assert
            var e = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await userBeforeSave.Save(username, null, null, null, null, null);
            });
            
            Assert.That(e, Is.Not.Null);
            Assert.That(e.Message, Is.EqualTo("No updates provided."));
        }

        [Test]
        public async Task Save_UpdateProvidedForName_UpdatesOnlyPassword()
        {
            // Arrange
            string username = "Testuser7";
            string password = "Testpassword";
            
            // Act
            await User.Create(username, password);
            var userBeforeSave = await User.GetUserByUserName(username);
            await userBeforeSave.Save(username, "Test", null, null, null, null);
            var userAfterSave = await User.GetUserByUserName(username);
            
            // Assert
            Assert.That(userBeforeSave.UserName, Is.EqualTo(username));
            Assert.That(userBeforeSave.Coins, Is.EqualTo(userAfterSave.Coins));
            Assert.That(userBeforeSave.ELO, Is.EqualTo(userAfterSave.ELO));
            Assert.That(userAfterSave.Name, Is.EqualTo("Test"));
            Assert.That(userBeforeSave.Bio, Is.EqualTo(userAfterSave.Bio));
            Assert.That(userBeforeSave.Image, Is.EqualTo(userAfterSave.Image));
        }

        [Test]
        public async Task BuyPackage_NoPackageExists_ThrowsException()
        {
            // Arrange
            string username = "Testuser8";
            string password = "Testpassword";
            
            // Act
            await User.Create(username, password);
            var user = await User.GetUserByUserName(username);
            
            // Assert
            var e = Assert.ThrowsAsync<Exception>(async () =>
            {
                await User.BuyPackage(user.UserName);
            });
            
            Assert.That(e, Is.Not.Null);
            Assert.That(e.Message, Is.EqualTo("Error selecting latest package: No package found."));
        }

        [Test]
        public async Task BuyPackage_NotEnoughCoins_ThrowsException()
        {
            // Arrange
            string username = "Testuser9";
            string password = "Testpassword";

            // Act 
            await User.Create(username, password);

            var user = await User.GetUserByUserName(username);
            await user.Save(username, null, null, null, null, 2); // Set coins to 2

            // Assert
            var ex = Assert.ThrowsAsync<UserException>(async () =>
            {
                await User.BuyPackage(username);
            });

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo($"User {username} does not have enough coins"));
        }
        
        [Test]
        public async Task BuyPackage_SuccessfullyBuysPackage()
        {
            string username = "Testuser9";
            string password = "Testpassword";

            await User.Create(username, password);

            await using var connection = new NpgsqlConnection("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg_test");
            await connection.OpenAsync();

            const int packageId = 1;
            await using var packageCmd = new NpgsqlCommand(
                "INSERT INTO \"package\" (id, isOpened) VALUES (@id, false);", connection);
            packageCmd.Parameters.AddWithValue("@id", packageId);
            await packageCmd.ExecuteNonQueryAsync();

            const string cardInsertQuery = 
                "INSERT INTO card (id, packageId, name, damage, elementType, isMonster, monsterType) VALUES "+
                "('card1', @packageId, 'FireDragon', 50, 'fire', true, 'dragon'), " +   
                "('card2', @packageId, 'WaterSpell', 30, 'water', false, NULL), " +
                "('card3', @packageId, 'Wizard', 40, 'normal', true, 'wizard'), " +
                "('card4', @packageId, 'FireElf', 20, 'fire', true, 'fireElf'), " +
                "('card5', @packageId, 'WaterGoblin', 25, 'water', true, 'goblin');";
            await using var cardCmd = new NpgsqlCommand(cardInsertQuery, connection);
            cardCmd.Parameters.AddWithValue("@packageId", packageId);
            await cardCmd.ExecuteNonQueryAsync();

            await User.BuyPackage(username);

            var user = await User.GetUserByUserName(username);
            Assert.That(user.Coins, Is.EqualTo(15));

            const string packageQuery = "SELECT isOpened FROM \"package\" WHERE id = @id;";
            await using var verifyPackageCmd = new NpgsqlCommand(packageQuery, connection);
            verifyPackageCmd.Parameters.AddWithValue("@id", packageId);
            var isOpened = (bool)await verifyPackageCmd.ExecuteScalarAsync();
            Assert.That(isOpened, Is.True);

            var userStack = await User.GetUserStack(username);
            Assert.That(userStack.Count, Is.EqualTo(5));
            Assert.Multiple(() =>
            {
                Assert.That(userStack.Any(c => c.Id == "card1"), Is.True);
                Assert.That(userStack.Any(c => c.Id == "card2"), Is.True);
                Assert.That(userStack.Any(c => c.Id == "card3"), Is.True);
                Assert.That(userStack.Any(c => c.Id == "card4"), Is.True);
                Assert.That(userStack.Any(c => c.Id == "card5"), Is.True);
            });
        }
    }
}