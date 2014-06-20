using System;
using System.Collections.Generic;
using System.Linq;
using WorkloadTest.Models;
using System.Web;

namespace WorkloadTest.ViewModels
{
    public class InstanceViewModel
    {
        public int Task_ID { get; set; }
        public bool Routine { get; set; }
        public bool Priority { get; set; }

        public int? CoE_ID { get; set; }
        public CoEs CoE { get; set; }
        public int? Analyst_ID { get; set; }

        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Requestor { get; set; }
        public double? Workload { get; set; }
        public int? Workload_Unit_ID { get; set; }
        public string Comment { get; set; }

        public DateTime? Request_Date { get; set; }

        public int? Count { get; set; }
        public int? Frequency { get; set; }
        public int? Period_ID { get; set; }

        public string Data_Source { get; set; }
        public string Report_Location { get; set; }

        public string User_Added { get; set; }
        public DateTime? Date_Added { get; set; }

        public int Instance { get; set; }
        public DateTime? Task_Date { get; set; }
        public DateTime? Task_Date_From { get; set; }
        public string Instance_Comment { get; set; }
        public int Exception_ID { get; set; }
    }

    public class InstanceListViewModel
    {
        public List<InstanceViewModel> allInstances { get; set; }
        public List<Analysts> allAnalysts { get; set; }
        public List<Workload_Units> allWorkload_Units { get; set; }
        public List<CoEs> allCoEs { get; set; }

    }
}