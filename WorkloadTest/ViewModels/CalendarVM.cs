using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkloadTest.Models;

namespace WorkloadTest.ViewModels
{
    public class CalendarViewModel
    {
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string task_id { get; set; }
        public string instance_id { get; set; }
        public string comment { get; set; }
        public string allDay { get; set; }
        public string analyst_id { get; set; }
        public string coe_id { get; set; }
        public string color { get; set; }
        public string priority { get; set; }
    }
}