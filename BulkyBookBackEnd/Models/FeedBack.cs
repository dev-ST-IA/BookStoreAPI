using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class FeedBack
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Book Book { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public string Text { get; set; }
    }
}
