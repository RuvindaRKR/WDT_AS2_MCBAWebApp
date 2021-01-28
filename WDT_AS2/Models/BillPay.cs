using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDT_AS2.Utilities;

namespace WDT_AS2.Models
{
    public record BillPay
    {
        public int BillPayID { get; init; }

        [ForeignKey("Account")]
        public int AccountNumber { get; init; }
        public virtual Account Account { get; init; }

        [ForeignKey("Payee")]
        public int PayeeID { get; init; }
        public virtual Payee Payee { get; init; }

        [Column(TypeName = "money")]
        public decimal Amount { get; init; }

        [StringLength(20)]
        public string Status { get; init; }

        public DateTime ScheduleDate { get; init; }

        public Period Period { get; init; }

        public DateTime ModifyDate { get; init; }
    }
}
