using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string Name { get; set; } = default!;

        public ICollection<Book> Books { get; set; }
    }
}
