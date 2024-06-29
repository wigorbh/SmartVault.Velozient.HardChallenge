using System.IO;
using System;
using SmartVault.Program.BusinessObjects;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace SmartVault.Program
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            WriteEveryThirdFileToFile(args[0]);
            GetAllFileSizes();
        }
        private static void GetAllFileSizes()
        {
            // pass the directory to get the files
            var directoryPath = "";
            long totalSize = 0;

            try
            {
                var files = Directory.GetFiles(directoryPath);

                foreach (var file in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing file {file}: {ex.Message}");
                    }
                }

                Console.WriteLine($"Total file size of all files in '{directoryPath}': {totalSize} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing directory {directoryPath}: {ex.Message}");
            }
        }


        private static void WriteEveryThirdFileToFile(string accountId)
        {
            string outputDirectory = Directory.GetCurrentDirectory();
            string outputFileName = "OutputFile.txt";
            string outputFilePath = Path.Combine(outputDirectory, outputFileName);

            var documents = GetDocumentsByAccountId(accountId);

            var thirdDocuments = documents.Where((doc, index) => (index + 1) % 3 == 0);

            using (var writer = new StreamWriter(outputFilePath))
            {
                foreach (var document in thirdDocuments)
                {
                    try
                    {
                        string documentContent = File.ReadAllText(document.FilePath);
                        if (documentContent.Contains("Smith Property"))
                        {
                            writer.WriteLine($"Document Name: {document.Name}");
                            writer.WriteLine(documentContent);
                            writer.WriteLine(); // Add a newline for separation
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading file {document.FilePath}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Output file '{outputFileName}' created at '{outputDirectory}'.");
        }

        // Example for fetching documents
        private static IEnumerable<Document> GetDocumentsByAccountId(string accountId)
        {
            // at this point we can get the information from the database as well
            //var documents = connection.Query<Document>("SELECT * FROM Document WHERE AccountId = @AccountId", new { AccountId = accountId });


            return new List<Document>
            {
                // Mock data for demonstration
                new Document { Name = "Doc1", FilePath = @"C:\Files\Doc1.txt" },
                new Document { Name = "Doc2", FilePath = @"C:\Files\Doc2.txt" },
                new Document { Name = "Doc3", FilePath = @"C:\Files\Doc3.txt" }
            };
        }
    }
}