using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WDT_AS2.Models
{
    public record Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerID { get; init; }

        [Required, StringLength(50)]
        public string Name { get; init; }

        [StringLength(50)]
        public string Address { get; init; }

        [StringLength(40)]
        public string City { get; init; }

        [StringLength(4)]
        public string PostCode { get; init; }

        public virtual List<Account> Accounts { get; init; }
    }
}
