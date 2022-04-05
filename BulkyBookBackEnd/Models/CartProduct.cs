using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class CartProduct : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Book Product { get; set; }
        [Required]

        [Range(minimum: 1, maximum: int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public float TotalPrice { get; private set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Product.Units < Quantity)
            {
                yield return new ValidationResult("Invalid Quantity");
            }
            if ((Product.Price * Quantity) != TotalPrice)
            {
                yield return new ValidationResult("Invalid Pricing");
            }
        }

        public CartProduct(Book book,int quantity)
        {
            this.Product = book;
            this.Quantity = quantity;
            this.TotalPrice = Product.Price * quantity;
        }

        public CartProduct()
        {

        }
    }
}
