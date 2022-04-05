using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBookBackEnd.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }
        public ICollection<CartProduct>? Products { get; set; }
        public bool IsOpen { get; set; } = true;
    }
}
