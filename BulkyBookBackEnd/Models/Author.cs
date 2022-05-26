using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<Book> Books { get; set; }

    }
}
