using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AdminWebApp.Utilities;

namespace AdminWebApp.Models
{
    public class Account
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Account Number")]
        public int AccountNumber { get; set; }

        [Display(Name = "Type")]
        public AccountType AccountType { get; set; }

        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }

        [Column(TypeName = "money")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        public DateTime ModifyDate { get; init; }

        public virtual List<Transaction> Transactions { get; set; }

        public virtual List<BillPay> BillPays { get; init; }
    }
}
