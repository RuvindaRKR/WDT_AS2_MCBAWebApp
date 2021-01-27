using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WDT_AS2.Models
{
    public enum Period
    {
        Monthly = 1,
        Quaterly = 2,
        OnceOff = 3,
    }

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

        public DateTime ScheduleDate { get; init; }

        public Period Period { get; init; }

        public DateTime TransactionTimeUtc { get; init; }
    }
}
