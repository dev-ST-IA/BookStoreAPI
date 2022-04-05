using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class Credentials
    {
        public int Id { get; set; }

        [Required]
        public string Password {  get; set; }

        [Required]
        public string? Salt {  get; set; } = "none";

        [Required]
        public User User { get; set; }
    }
}
