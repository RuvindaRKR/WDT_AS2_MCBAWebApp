using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBAWebApplication.Models
{
    public record Payee
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PayeeID { get; init; }

        [Required, StringLength(50)]
        public string PayeeName { get; init; }

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

        public virtual List<BillPay> BillPays { get; init; }
    }
}
