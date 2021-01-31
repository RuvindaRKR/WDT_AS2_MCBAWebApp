using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MCBAWebApplication.Utilities;

namespace MCBAWebApplication.Models
{
    public class BillPay
    {     
        public int BillPayID { get; set; }

        [ForeignKey("Account")]
        public int AccountNumber { get; set; }
        public virtual Account Account { get; set; }

        [ForeignKey("Payee")]
        public int PayeeID { get; set; }
        public virtual Payee Payee { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public Status Status { get; set; }

        public DateTime ScheduleDate { get; set; }

        public Period Period { get; set; }

        public DateTime ModifyDate { get; set; }
    }
}
