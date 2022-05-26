using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Models
{
    public class User : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Password { private protected get;  set; }

        public string? Salt {private protected get; set; } = "none";

        public void setPasswordAndSalt(string password,string salt)
        {
            this.Password = password;
            this.Salt = salt;
        }

        public string getPassword()
        {
            return this.Password;
        }
        [Required]
        public string UserName { get; set; } = default!;


        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; } = default!;

        [Required]
        public string? Role { get; set; } = "Customer";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<string> allowedVals = new List<string>() { "Customer", "Administrator" };
            if (!allowedVals.Contains(Role))
            {
                yield return new ValidationResult($"Invalid Order Status {Role}");
            }
        }

        [Required]
        public int PhoneNumber { get; set; } = default!;

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        public ICollection<Order>? Orders { get; set; } = default!;

        public ICollection<Book>? WatchList { get; set; } = default!;

        public ICollection<FeedBack>? Feedbacks { get; set; } = default!;

        public ICollection<BookRating>? BookRatings { get; set; } = default!;

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public DateTime LastUpdatedDateTime { get; set; } = DateTime.Now;
    }
}
