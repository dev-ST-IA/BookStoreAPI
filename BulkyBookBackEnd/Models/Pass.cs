using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace BulkyBookBackEnd.Models
{
    public class Pass
    {
        public int Id { get; set; }

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        public string Salt { get; set; } =default!; 

    }
}

//{
//    "userName": "salman",
//"password":"salman",
//  "emailAddress": "user@example.com",
//  "role": "Customer",
//  "phoneNumber": 123456789,
//  "firstName": "salman",
//  "lastName": "salman"
//}
