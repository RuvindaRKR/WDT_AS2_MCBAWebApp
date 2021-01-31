// Code adapted from https://www.yogihosting.com/aspnet-core-identity-roles/#create-role
// Last Updated: 18-07-20
// Accessed: 31-01-21
using System.ComponentModel.DataAnnotations;

namespace MCBAWebApplication.Models
{
    public class RoleModification
    {
        [Required]
        public string RoleName { get; set; }

        public string RoleId { get; set; }

        public string[] AddIds { get; set; }

        public string[] DeleteIds { get; set; }
    }
}