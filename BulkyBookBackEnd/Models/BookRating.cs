using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBookBackEnd.Models
{
    public class BookRating
    {
        [Key]
        public int Id { get; set; }

        public int BookId   { get; set; }

        [Required]
        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [Required]
        public User? User { get; set; }

        [Range(minimum: 0, maximum: 10)]
        public double Rating { get; set; } = 0;

    }
}
