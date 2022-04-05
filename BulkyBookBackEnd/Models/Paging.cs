using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class Paging
    {
        public int Size { get; set; } = 10;

        public int Page { get; set; } = 1;

        //sort = popular, name = asc,desc , date = asc,desc,price = asc,desc

        public string? Sort { get; set; } = "popular";

    }
}
