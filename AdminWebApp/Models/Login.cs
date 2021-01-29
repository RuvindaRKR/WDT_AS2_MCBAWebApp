using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminWebApp.Models
{
    public record Login
    {
        [Required, StringLength(8)]
        [Display(Name = "Login ID")]
        public string LoginID { get; init; }

        public int CustomerID { get; init; }
        public virtual Customer Customer { get; init; }

        [Required, StringLength(64)]
        public string PasswordHash { get; init; }

        public DateTime ModifyDate { get; init; }
    }
}
