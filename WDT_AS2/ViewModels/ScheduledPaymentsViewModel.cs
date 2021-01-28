using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using WDT_AS2.Utilities;

namespace WDT_AS2.ViewModels
{
    public class ScheduledPaymentsViewModel
    {
        public int BillPayID { get; init; }
        public string PayeeName { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; init; }

        public Status Status { get; init; }

        public DateTime ScheduleDate { get; init; }

        public Period Period { get; init; }
    }
}
