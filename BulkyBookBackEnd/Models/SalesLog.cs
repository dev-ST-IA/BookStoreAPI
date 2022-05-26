using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class SalesLog : IValidatableObject
    {
        [Key]
        public int Id { get; set; }
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public int Day { get; set; } = DateTime.Now.Day;
        public int Month { get; set; } = DateTime.Now.Month;
        public int Year { get; set; } = DateTime.Now.Year;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Day<0 || Day > 31)
            {
                yield return new ValidationResult($"Invalid Day");
            }
            if (Month < 1|| Month > 12)
            {
                yield return new ValidationResult($"Invalid Month");
            }
            if (Year < 2022 || Year > DateTime.MaxValue.Year || !(Year.ToString().Length==4))
            {
                yield return new ValidationResult($"Invalid Day");
            }
        }
    }
}
