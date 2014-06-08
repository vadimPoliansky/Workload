using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkloadTest.Models;

namespace WorkloadTest.ViewModels
{
    public class RecurrenceViewModel
    {
        public int Instance_ID { get; set; }
        public int? Exception_ID { get; set; }
        public DateTime Task_Date { get; set; }
        public DateTime? New_Task_Date { get; set; }
        public bool Canceled { get; set; }
        public string Instance_Comment { get; set; }

    }

    public class RecurrenceListViewModel
    {
        public List<RecurrenceViewModel> allRecurrences { get; set; }
    }
}