using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBookBackEnd.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Category Category { get; set; } = default!;

        [Required]
        public Author Author { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        public string Description { get; set; } = default!;

        [Required]
        public float Price { get; set; }

        [Required]
        public float Cost { get; set; }

        public ICollection<FeedBack> FeedBacks { get; set; } = default!;

        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Must be greater than or equal to {0}")]
        public int Units { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public ICollection<BookRating>  Ratings { get; set; } = default!;

        public double FinalRating { get; set; } = 0;

        public string ImageUrl { get; set; } = string.Empty;

        public string ImageName { get; set; } = string.Empty;

        public string Publisher { get; set; } = string.Empty;

        public int Sales { get; set; } = 0;

    }
}
