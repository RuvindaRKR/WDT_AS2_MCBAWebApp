// Code adapted from https://www.yogihosting.com/aspnet-core-identity-roles/#create-role
// Last Updated: 18-07-20
// Accessed: 31-01-21
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MCBAWebApplication.Models
{
    public class RoleEdit
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<ApplicationUser> Members { get; set; }
        public IEnumerable<ApplicationUser> NonMembers { get; set; }
    }
}