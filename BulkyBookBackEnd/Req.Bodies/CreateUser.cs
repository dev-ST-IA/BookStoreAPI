﻿using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
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

        public bool checkIfExist(BookDbContext db)
        {
            var existingUser = db.Users.Any(u=>
            (u.UserName == UserName) ||
            (u.EmailAddress == EmailAddress && Role==u.Role) ||
            (u.PhoneNumber==PhoneNumber&& Role==u.Role)
            );
            return existingUser;
        }

    }
}
