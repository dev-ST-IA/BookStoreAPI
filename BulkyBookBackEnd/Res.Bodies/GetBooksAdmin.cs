using BulkyBookBackEnd.Models;

namespace BulkyBookBackEnd.Res.Bodies
{
    public class GetBooksAdmin
    {
        public Book Book { get; set; }
        public string CategoryName { get; set; }

        public int CategoryId { get; set; }

        public string? AuthorName { get; set; }

        public int? AuthorId { get; set; }
    }
}
