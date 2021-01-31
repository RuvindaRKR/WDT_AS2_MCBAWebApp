using MCBAWebApplication.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBAWebApplication.Models
{
    public record Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerID { get; init; }

        [Required, StringLength(50)]
        public string CustomerName { get; init; }

        [StringLength(11)]
        public string TFN { get; init; }

        [StringLength(50)]
        public string Address { get; init; }

        [StringLength(40)]
        public string City { get; init; }

        [StringLength(20)]
        public string State { get; init; }

        [StringLength(10)]
        public string PostCode { get; init; }

        [StringLength(15)]
        public string Phone { get; init; }

        public AccountStatus AccountStatus { get; init; }

        public virtual List<Account> Accounts { get; init; }
    }
}
