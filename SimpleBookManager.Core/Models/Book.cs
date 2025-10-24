using System.Xml.Serialization;

namespace SimpleBookManager.Core.Models
{
    [XmlRoot("book")]
    public class Book
    {
        [XmlElement("title")]
        public string Title { get; set; } = string.Empty;

        [XmlElement("author")]
        public string Author { get; set; } = string.Empty;

        [XmlElement("pages")]
        public int Pages { get; set; }

        public Book() { }

        public Book(string title, string author, int pages)
        {
            Title = title;
            Author = author;
            Pages = pages;
        }
    }
}
