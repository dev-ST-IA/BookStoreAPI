using BulkyBookBackEnd.Models;

namespace BulkyBookBackEnd.Res.Bodies
{
    public class GetBooks
    {
        public PaginatedList<Book> Books { get; set; }
        public int TotalPages { get; set; }
    }
}
