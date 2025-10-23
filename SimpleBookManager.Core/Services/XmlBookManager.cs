using SimpleBookManager.Core.Models;
using SimpleBookManager.Core.Services.Interfaces;
using System.Xml.Linq;

namespace SimpleBookManager.Core.Services
{
    public class XmlBookManager : IBookManager
    {
        private readonly string _xmlFilePath;
        private readonly List<Book> _books = new();

        public XmlBookManager(string xmlFilePath)
        {
            _xmlFilePath = xmlFilePath;

            // todo: maybe do not create file if no file found?
            // ideas:
            // 1) throw exception;
            // 2) Do not do anything - manager will have empty book list, xml file will be created later when Save method is called
            if (!File.Exists(_xmlFilePath))
            {
                var newDoc = new XDocument(new XElement("books"));
                newDoc.Save(_xmlFilePath);
            }

            var doc = XDocument.Load(_xmlFilePath);

            _books.AddRange(doc.Root?.Elements("book")
                .Select(b => new Book()
                {
                    Title = b.Element("title")?.Value ?? string.Empty,
                    Author = b.Element("author")?.Value ?? string.Empty,
                    Pages = int.TryParse(b.Element("pages")?.Value, out var result) ? result : 0
                }) ?? Enumerable.Empty<Book>());
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

            return _books; // todo: should the list be returned besides beign sorted?
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
            var doc = new XDocument(new XElement("books", _books
                .Select(b => new XElement("book",
                    new XElement("title", b.Title),
                    new XElement("author", b.Author),
                    new XElement("pages", b.Pages)))));

            doc.Save(filepath);
        }
    }
}
