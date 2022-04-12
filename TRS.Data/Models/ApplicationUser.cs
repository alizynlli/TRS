using Microsoft.AspNetCore.Identity;
using System;

namespace TRS.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Guid? ClientId { get; set; }
        public Client Client { get; set; }
    }
}
