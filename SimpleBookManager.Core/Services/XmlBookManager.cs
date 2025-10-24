using System.Xml.Serialization;
using SimpleBookManager.Core.Models;
using SimpleBookManager.Core.Services.Interfaces;

namespace SimpleBookManager.Core.Services
{
    public class XmlBookManager : IBookManager
    {
        private readonly string _xmlFilePath;
        private readonly List<Book> _books = new();

        public XmlBookManager(string xmlFilePath, bool createIfNotExists = false)
        {
            _xmlFilePath = xmlFilePath;

            if (!File.Exists(_xmlFilePath))
            {
                if (!createIfNotExists)
                {
                    throw new FileNotFoundException($"The file '{_xmlFilePath}' was not found.");
                }

                var emptyCatalogue = new BooksCatalogue();
                var serializer = new XmlSerializer(typeof(BooksCatalogue));

                using (var stream = File.Create(_xmlFilePath))
                {
                    serializer.Serialize(stream, emptyCatalogue);
                }
            }

            using (var stream = File.OpenRead(_xmlFilePath))
            {
                var serializer = new XmlSerializer(typeof(BooksCatalogue));
                if (serializer.Deserialize(stream) is BooksCatalogue catalogue && catalogue.Books != null)
                {
                    _books.AddRange(catalogue.Books);
                }
            }
        }

        public List<Book> GetBooks() => _books;

        public void AddBook(Book book) => _books.Add(book);

        public List<Book> SortAlphabetically()
        {
            _books.Sort((a, b) =>
            {
                var authorCompare = string.Compare(a.Author, b.Author, true);
                if (authorCompare == 0)
                    return string.Compare(a.Title, b.Title, true);
                return authorCompare;
            });

            return _books;
        }

        public List<Book> SearchByName(string bookName)
        {
            return _books.Where(b => b.Title.Contains(bookName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public void SaveBooks()
        {
            SaveBooks(_xmlFilePath);
        }

        public void SaveBooks(string filepath)
        {
            var serializer = new XmlSerializer(typeof(BooksCatalogue));
            var catalogue = new BooksCatalogue { Books = _books };

            using (var stream = File.Create(filepath))
            {
                serializer.Serialize(stream, catalogue);
            }
        }
    }
}
