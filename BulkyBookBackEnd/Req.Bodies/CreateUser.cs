using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Req.Bodies
{
    public class CreateUser
    {

        [Required]
        public string Password { get; set; } = default!;


        [Required]
        public string UserName { get; set; } = default!;


        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; } = default!;

        [Required]
        public string? Role { get; set; } = "Customer";

        [Required]
        public int PhoneNumber { get; set; } = default!;

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

    }
}
