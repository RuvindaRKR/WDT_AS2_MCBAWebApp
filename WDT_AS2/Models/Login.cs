using System.ComponentModel.DataAnnotations;

namespace WDT_AS2.Models
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
    }
}
