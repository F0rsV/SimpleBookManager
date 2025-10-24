using System.Xml.Serialization;
using SimpleBookManager.Core.Models;
using SimpleBookManager.Core.Services;

namespace SimpleBookManager.Tests
{
    public class XmlBookManagerTests
    {
        private const string TestFileName = "BooksTestLib.xml";

        private readonly string _testFilePath;
        private readonly string _tempFilePath;

        public XmlBookManagerTests()
        {
            _testFilePath = Path.Combine(Directory.GetCurrentDirectory(), TestFileName);
            _tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TempBooks.xml");

            if (!File.Exists(_testFilePath))
                throw new FileNotFoundException($"{TestFileName} not found", _testFilePath);

            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }

        [Fact]
        public void Constructor_ExistingFile_LoadsBooksFromFile()
        {
            var manager = new XmlBookManager(_testFilePath);

            var books = manager.GetBooks();
            Assert.NotEmpty(books);
            Assert.Contains(books, b => b.Title == "The Hobbit");
            Assert.Contains(books, b => b.Author == "Stephen King");
        }

        [Fact]
        public void Constructor_FileDoesNotExistWithoutFlag_ThrowsException()
        {
            var missingPath = Path.Combine(Directory.GetCurrentDirectory(), "NonExistingFile.xml");

            if (File.Exists(missingPath))
                File.Delete(missingPath);

            Assert.Throws<FileNotFoundException>(() => new XmlBookManager(missingPath));
        }

        [Fact]
        public void Constructor_FileDoesNotExistWithFlag_CreatesFileEmptyCatalogue()
        {
            var missingPath = Path.Combine(Directory.GetCurrentDirectory(), "NewEmptyFile.xml");

            if (File.Exists(missingPath))
                File.Delete(missingPath);

            var manager = new XmlBookManager(missingPath, true);

            Assert.True(File.Exists(missingPath));
            Assert.Empty(manager.GetBooks());

            File.Delete(missingPath);
        }

        [Fact]
        public void Constructor_InvalidXml_ThrowsInvalidDataException()
        {
            var invalidXmlPath = Path.Combine(Directory.GetCurrentDirectory(), "InvalidFile.xml");
            File.WriteAllText(invalidXmlPath, "<NotBooksCatalogue></NotBooksCatalogue>");

            Assert.Throws<InvalidDataException>(() => new XmlBookManager(invalidXmlPath));

            File.Delete(invalidXmlPath);
        }

        [Fact]
        public void Constructor_EmptyFile_ThrowsInvalidDataException()
        {
            var emptyPath = Path.Combine(Directory.GetCurrentDirectory(), "EmptyFile.xml");
            File.WriteAllText(emptyPath, string.Empty);

            Assert.Throws<InvalidDataException>(() => new XmlBookManager(emptyPath));

            File.Delete(emptyPath);
        }

        [Fact]
        public void AddBook_ValidBook_AddsBookToInternalList()
        {
            var manager = new XmlBookManager(_testFilePath);
            int initialCount = manager.GetBooks().Count;

            var newBook = new Book("Test Book", "Test Author", 123);
            manager.AddBook(newBook);

            var books = manager.GetBooks();
            Assert.Equal(initialCount + 1, books.Count);
            Assert.Contains(books, b => b.Title == "Test Book" && b.Author == "Test Author");
        }

        [Fact]
        public void GetBooks_CalledMultipleTimes_ReturnsSameListReference()
        {
            var manager = new XmlBookManager(_testFilePath);
            var books1 = manager.GetBooks();
            var books2 = manager.GetBooks();

            Assert.Same(books1, books2);
        }

        [Fact]
        public void SortAlphabetically_MultipleBooks_SortsByAuthorThenTitle()
        {
            var manager = new XmlBookManager(_testFilePath);

            var sorted = manager.SortAlphabetically();

            var expectedOrder = sorted
                .OrderBy(b => b.Author, StringComparer.OrdinalIgnoreCase)
                .ThenBy(b => b.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Assert.Equal(expectedOrder.Select(b => b.Title), sorted.Select(b => b.Title));
        }

        [Fact]
        public void SortAlphabetically_MixedCaseAuthorsAndTitles_SortsCaseInsensitively()
        {
            var manager = new XmlBookManager(_testFilePath);
            manager.AddBook(new Book("aTitle", "aAuthor", 100));
            manager.AddBook(new Book("Btitle", "Aauthor", 100));

            var sorted = manager.SortAlphabetically();
            var first = sorted.First();

            Assert.Equal("aAuthor", first.Author, ignoreCase: true);
        }

        [Fact]
        public void SearchByName_TitleContainsSubstring_ReturnsMatchingBooks()
        {
            var manager = new XmlBookManager(_testFilePath);
            var results = manager.SearchByName("The");

            Assert.NotEmpty(results);
            Assert.All(results, b => Assert.Contains("The", b.Title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SearchByName_NoMatchingTitle_ReturnsEmptyList()
        {
            var manager = new XmlBookManager(_testFilePath);
            var results = manager.SearchByName("NonexistentTitle");

            Assert.Empty(results);
        }

        [Fact]
        public void SearchByName_CaseVariationProvided_ReturnsSameResults()
        {
            var manager = new XmlBookManager(_testFilePath);
            var upper = manager.SearchByName("THE");
            var lower = manager.SearchByName("the");

            Assert.Equal(upper.Count, lower.Count);
        }

        [Fact]
        public void SaveBooks_ValidPath_CreatesOrOverwritesFile()
        {
            var manager = new XmlBookManager(_testFilePath);
            manager.SaveBooks(_tempFilePath);

            Assert.True(File.Exists(_tempFilePath));

            BooksCatalogue catalogue;
            using (var stream = File.OpenRead(_tempFilePath))
            {
                var serializer = new XmlSerializer(typeof(BooksCatalogue));
                catalogue = (BooksCatalogue)serializer.Deserialize(stream)!;
            }

            Assert.NotNull(catalogue);
            Assert.NotNull(catalogue.Books);
        }

        [Fact]
        public void SaveBooks_AfterAddingBook_NewBookSavedToFile()
        {
            var manager = new XmlBookManager(_testFilePath);
            var newBook = new Book("New Book", "New Author", 227);
            manager.AddBook(newBook);

            manager.SaveBooks(_tempFilePath);

            BooksCatalogue catalogue;
            using (var stream = File.OpenRead(_tempFilePath))
            {
                var serializer = new XmlSerializer(typeof(BooksCatalogue));
                catalogue = (BooksCatalogue)serializer.Deserialize(stream)!;
            }

            Assert.Contains(catalogue.Books, b => b.Title == "New Book" && b.Author == "New Author" && b.Pages == 227);
        }

        [Fact]
        public void SaveBooks_NoPathProvided_UsesDefaultFilePath()
        {
            var copyPath = Path.Combine(Directory.GetCurrentDirectory(), "CopyBooks.xml");
            File.Copy(_testFilePath, copyPath, true);

            var manager = new XmlBookManager(copyPath);
            var newBook = new Book("New Book", "New Author", 227);
            manager.AddBook(newBook);

            manager.SaveBooks();

            BooksCatalogue catalogue;
            using (var stream = File.OpenRead(copyPath))
            {
                var serializer = new XmlSerializer(typeof(BooksCatalogue));
                catalogue = (BooksCatalogue)serializer.Deserialize(stream)!;
            }

            Assert.Contains(catalogue.Books, b => b.Title == "New Book");
            File.Delete(copyPath);
        }
    }
}
