using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using SmartVault.DataGeneration;
using System.Data.SQLite;

namespace SmartVault.DataGeneration.Tests
{
    [TestFixture]
    public class ProgramTests
    {

        private SQLiteConnection _connection;
        private string _testDatabaseFile = "test_database.db";

        [TearDown]
        public void Teardown()
        {
            // Clean up
            _connection.Close();
            File.Delete(_testDatabaseFile);
        }

        [SetUp]
        public void Setup()
        {
            // Create a SQLite database for testing
            SQLiteConnection.CreateFile(_testDatabaseFile);
            _connection = new SQLiteConnection($"Data Source={_testDatabaseFile};Version=3;");
            _connection.Open();

            // Create test schema and populate with data
            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS Account (Id INTEGER PRIMARY KEY, Name TEXT, CreatedOn DATETIME);
                CREATE TABLE IF NOT EXISTS Document (Id INTEGER PRIMARY KEY, Name TEXT, FilePath TEXT, Length INTEGER, AccountId INTEGER, CreatedOn DATETIME);
                CREATE TABLE IF NOT EXISTS User (Id INTEGER PRIMARY KEY, FirstName TEXT, LastName TEXT, DateOfBirth TEXT, AccountId INTEGER, Username TEXT, Password TEXT, CreatedOn DATETIME);
            ";
            var populateDataSql = @"
                INSERT INTO Account (Id, Name, CreatedOn) VALUES (1, 'Account-1', '2024-06-30');
                INSERT INTO Document (Id, Name, FilePath, Length, AccountId, CreatedOn) VALUES (1, 'Document1.txt', 'C:\Test\Document1.txt', 1024, 1, '2024-06-30');
                INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password, CreatedOn) VALUES (1, 'John', 'Doe', '1990-01-01', 1, 'johndoe', 'password', '2024-06-30');
            ";

            using (var cmd = new SQLiteCommand(createTableSql, _connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(populateDataSql, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }


        [Test]
        public void GenerateTestDocument_WritesCorrectContentToFile()
        {
            // Arrange

            // Clear existing file if it exists
            var filePath = "TestDoc.txt";
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act
            Program.GenerateTestDocument();

            // Assert
            Assert.IsTrue(File.Exists(filePath), "File should be created");

            var content = File.ReadAllText(filePath);
            var expectedContent = $"This is my test document {Environment.NewLine}";
            var repeatedContent = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                repeatedContent.Append(expectedContent);
            }

            Assert.That(content, Is.EqualTo(repeatedContent.ToString()), "File content should match expected");
        }

        [Test]
        public void PrintDatabaseStatistics_ReturnsCorrectCounts()
        {
            // Arrange
            var connection = new SQLiteConnection($"Data Source={_testDatabaseFile};Version=3;");
            connection.Open();

            // Act
            Program.PrintDatabaseStatistics(connection);

            // Assert
            var expectedAccountCount = 1;
            var expectedDocumentCount = 1;
            var expectedUserCount = 1;

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Account;", connection))
            {
                var accountCount = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.That(accountCount, Is.EqualTo(expectedAccountCount), $"Expected Account count: {expectedAccountCount}, Actual: {accountCount}");
            }

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Document;", connection))
            {
                var documentCount = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.That(documentCount, Is.EqualTo(expectedDocumentCount), $"Expected Document count: {expectedDocumentCount}, Actual: {documentCount}");
            }

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM User;", connection))
            {
                var userCount = Convert.ToInt32(cmd.ExecuteScalar());
                Assert.That(userCount, Is.EqualTo(expectedUserCount), $"Expected User count: {expectedUserCount}, Actual: {userCount}");
            }
        }
    }
}
