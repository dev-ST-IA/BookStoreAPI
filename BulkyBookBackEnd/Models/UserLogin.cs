using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class UserLogin:IValidatableObject
    {
        [Required]
        public string Password { get; set; } = default!;

        public string? UserName { get; set; } = default!;

        [EmailAddress]
        public string EmailAddress { get; set; } = default!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UserName==null&&EmailAddress==null)
            {
                yield return new ValidationResult("Specify username or email");
            }
        }
    }
}
