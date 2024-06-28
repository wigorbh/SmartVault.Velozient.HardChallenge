using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Xml.Serialization;
using SmartVault.Library;
using System.Data.SQLite;
using System.Text;
using System.IO;
using Dapper;
using System;
using SmartVault.Program.BusinessObjects;

namespace SmartVault.DataGeneration
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            SQLiteConnection.CreateFile(configuration["DatabaseFileName"]);
            GenerateTestDocument();

            var connection = new SQLiteConnection(configuration["ConnectionStrings:DefaultConnection"]);
            LoadBusinessObjectDefinitions(connection);
            PopulateTestData(connection);
            PrintDatabaseStatistics(connection);
        }

        private static void GenerateTestDocument()
        {
            int repeatCount = 100;
            string documentContent = $"This is my test document {Environment.NewLine}";

            var fullContent = new StringBuilder();

            for (int i = 0; i < repeatCount; i++)
            {
                fullContent.Append(documentContent);
            }

            File.WriteAllText("TestDoc.txt", fullContent.ToString());
        }


        static void LoadBusinessObjectDefinitions(SQLiteConnection connection)
        {
            var businessObjectSchemaPath = @"..\..\..\..\BusinessObjectSchema";
            var xmlFiles = Directory.GetFiles(businessObjectSchemaPath);

            var serializer = new XmlSerializer(typeof(BusinessObject));

            foreach (var xmlFile in xmlFiles)
            {
                var reader = new StreamReader(xmlFile);
                var businessObject = serializer.Deserialize(reader) as BusinessObject;
                connection.Execute(businessObject?.Script);
            }
        }


        private static void PrintDatabaseStatistics(SQLiteConnection connection)
        {
            var accountCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Account;");
            Console.WriteLine($"Number of Accounts: {accountCount}");

            var documentCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM Document;");
            Console.WriteLine($"Number of Documents: {documentCount}");

            var userCount = connection.QuerySingle<int>("SELECT COUNT(*) FROM User;");
            Console.WriteLine($"Number of Users: {userCount}");
        }


        static void PopulateTestData(SQLiteConnection connection)
        {
            connection.Open();
            connection.Execute("PRAGMA synchronous = OFF");
            connection.Execute("PRAGMA journal_mode = MEMORY");

            int numUsers = 100;
            int numDocumentsPerUser = 1000;

            var transaction = connection.BeginTransaction();

            var documentNumber = 0;
            var randomDayIterator = RandomDay().GetEnumerator();

            var userInserts = new List<User>();
            var accountInserts = new List<Account>();
            var documentInserts = new List<Document>();


            for (int i = 0; i < numUsers; i++)
            {
                randomDayIterator.MoveNext();
                DateTime createdOn = DateTime.UtcNow;

                // Create User object
                var user = new User
                {
                    Id = i,
                    FirstName = $"FirstName-{i}",
                    LastName = $"LastName-{i}",
                    DateOfBirth = randomDayIterator.Current,
                    AccountId = i,
                    Username = $"UserName-{i}",
                    Password = "e10adc3949ba59abbe56e057f20f883e",
                    CreatedOn = createdOn // Set the created on date
                };
                userInserts.Add(user);

                // Create Account object
                var account = new Account
                {
                    Id = i,
                    Name = $"Account-{i}",
                    CreatedOn = createdOn // Set the created on date
                };
                accountInserts.Add(account);

                var documentPath = new FileInfo("TestDoc.txt").FullName;

                // Create Document objects
                for (int d = 0; d < numDocumentsPerUser; d++, documentNumber++)
                {
                    var document = new Document
                    {
                        Id = documentNumber,
                        Name = $"Document{i}-{d}.txt",
                        FilePath = documentPath,
                        Length = new FileInfo(documentPath).Length,
                        AccountId = i,
                        CreatedOn = createdOn
                    };
                    documentInserts.Add(document);
                }
            }

            connection.Execute("INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password) VALUES (@Id, @FirstName, @LastName, @DateOfBirth, @AccountId, @Username, @Password)", userInserts, transaction);
            connection.Execute("INSERT INTO Account (Id, Name) VALUES (@Id, @Name)", accountInserts, transaction);
            connection.Execute("INSERT INTO Document (Id, Name, FilePath, Length, AccountId) VALUES (@Id, @Name, @FilePath, @Length, @AccountId)", documentInserts, transaction);

            transaction.Commit();
        }

        static IEnumerable<DateTime> RandomDay()
        {
            DateTime start = new DateTime(1985, 1, 1);
            Random gen = new Random();
            int range = (DateTime.Today - start).Days;
            while (true)
                yield return start.AddDays(gen.Next(range));
        }
    }
}
