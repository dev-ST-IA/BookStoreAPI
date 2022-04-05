using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBookBackEnd.Models
{
    public class Order : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public ICollection<CartProduct> CartProducts { get; set; }


        [Required]
        public float TotalPrice { get; set; }

        [Required]
        public int TotalSales { get; set; }

        public DateTime OrderDate { get; set; }= DateTime.Now;
        
        [Required]
        public string OrderStatus { get; set; } = "Ordered";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<string> allowedVals = new List<string>() { "Ordered","Cancelled","Delivered","Returned"};
            if (!allowedVals.Contains(OrderStatus))
            {
                yield return new ValidationResult($"Invalid Order Status {OrderStatus}");
            }
        }

        [Required]
        public DateTime OrderUpdateDate { get; set; } = DateTime.Now;

    }

}
