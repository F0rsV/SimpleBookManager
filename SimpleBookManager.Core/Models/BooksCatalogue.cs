using System.Xml.Serialization;

namespace SimpleBookManager.Core.Models
{
    [XmlRoot("books")]
    public class BooksCatalogue
    {
        [XmlElement("book")]
        public List<Book> Books { get; set; } = new();
    }
}
