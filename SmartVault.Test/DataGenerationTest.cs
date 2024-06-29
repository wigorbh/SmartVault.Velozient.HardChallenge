using System.Text;

namespace SmartVault.DataGeneration.Tests
{
    [TestFixture]
    public class ProgramTests
    {
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
    }
}
