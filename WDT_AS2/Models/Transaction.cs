using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WDT_AS2.Utilities;

namespace WDT_AS2.Models
{
    public record Transaction
    {
        public int TransactionID { get; init; }

        public TransactionType TransactionType { get; init; }

        [ForeignKey("Account")]
        public int AccountNumber { get; init; }
        public virtual Account Account { get; init; }

        [ForeignKey("DestinationAccount")]
        public int? DestinationAccountNumber { get; init; }
        public virtual Account DestinationAccount { get; init; }

        [Column(TypeName = "money")]
        public decimal Amount { get; init; }

        [StringLength(255)]
        public string Comment { get; init; }

        public DateTime TransactionTimeUtc { get; init; }
    }
}
