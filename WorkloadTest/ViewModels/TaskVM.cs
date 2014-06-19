using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkloadTest.Models;

namespace WorkloadTest.ViewModels
{
    public class TaskViewModel
    {
        public int Task_ID { get; set; }
        public bool Routine { get; set; }
        public bool Priority { get; set; }

        public int? CoE_ID { get; set; }
        public int? Analyst_ID { get; set; }

        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Requestor { get; set; }
        public double? Workload { get; set; }
        public int? Workload_Unit_ID { get; set; }
        public string Comment { get; set; }

        public string Data_Source { get; set; }
        public string Report_Location { get; set; }

        public string Start_Date { get; set; }
        public string Request_Date { get; set; }

        public int? Count { get; set; }
        public int? Frequency { get; set; }
        public int? Period_ID { get; set; }

        public string User_Added { get; set; }
        public string Date_Added { get; set; }

        public bool? Saved { get; set; }

        public string Analyst { get; set; }
        public string CoE { get; set; }
        public string Period { get; set; }
    }

    public class TaskListViewModel
    {
        public List<TaskViewModel> allTasks { get; set; }
        public List<CoEs> allCoEs { get; set; }
        public List<Analysts> allAnalysts { get; set; }
    }
}