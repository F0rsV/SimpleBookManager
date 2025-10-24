using System.Xml.Serialization;
using SimpleBookManager.Core.Models;
using SimpleBookManager.Core.Services.Interfaces;

namespace SimpleBookManager.Core.Services
{
    public class XmlBookManager : IBookManager
    {
        private readonly string _xmlFilePath;
        private readonly BooksCatalogue _booksCatalogue = new();

        public XmlBookManager(string xmlFilePath, bool createIfNotExists = false)
        {
            _xmlFilePath = xmlFilePath;

            if (!File.Exists(_xmlFilePath))
            {
                if (!createIfNotExists)
                {
                    throw new FileNotFoundException($"The file '{_xmlFilePath}' was not found.");
                }

                var serializer = new XmlSerializer(typeof(BooksCatalogue));

                using (var stream = File.Create(_xmlFilePath))
                {
                    serializer.Serialize(stream, _booksCatalogue);
                }
            }

            using (var stream = File.OpenRead(_xmlFilePath))
            {
                var serializer = new XmlSerializer(typeof(BooksCatalogue));

                BooksCatalogue? catalogue;
                try
                {
                    catalogue = serializer.Deserialize(stream) as BooksCatalogue;
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidDataException("Invalid file format", ex);
                }

                if (catalogue == null)
                {
                    throw new InvalidDataException("Invalid file format");
                }

                _booksCatalogue.Books.AddRange(catalogue.Books);
            }

        }

        public List<Book> GetBooks() => _booksCatalogue.Books;

        public void AddBook(Book book) => _booksCatalogue.Books.Add(book);

        public List<Book> SortAlphabetically()
        {
            _booksCatalogue.Books.Sort((a, b) =>
            {
                var authorCompare = string.Compare(a.Author, b.Author, true);
                if (authorCompare == 0)
                    return string.Compare(a.Title, b.Title, true);
                return authorCompare;
            });

            return _booksCatalogue.Books;
        }

        public List<Book> SearchByName(string bookName)
        {
            return _booksCatalogue.Books.Where(b => b.Title.Contains(bookName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public void SaveBooks()
        {
            SaveBooks(_xmlFilePath);
        }

        public void SaveBooks(string filepath)
        {
            var serializer = new XmlSerializer(typeof(BooksCatalogue));

            using (var stream = File.Create(filepath))
            {
                serializer.Serialize(stream, _booksCatalogue);
            }
        }
    }
}
