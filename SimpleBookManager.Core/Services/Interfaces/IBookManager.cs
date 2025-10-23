using SimpleBookManager.Core.Models;

namespace SimpleBookManager.Core.Services.Interfaces
{
    public interface IBookManager
    {
        List<Book> GetBooks();
        void AddBook(Book book);
        List<Book> SortAlphabetically();
        List<Book> SearchByName(string bookName);
        void SaveBooks();
        void SaveBooks(string filepath);

        // todo: methods that could be implemented in future:
        // UpdateBook, RemoveBook
    }
}
