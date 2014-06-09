using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkloadTest.Models;

namespace WorkloadTest.ViewModels
{
    public class CreateViewModel
    {
        public Tasks task { get; set; }
        public List<CoEs> allCoEs { get; set; }
        public List<Analysts> allAnalysts { get; set; }
        public List<Workload_Units> allWorkload_Units { get; set; }
        public List<Periods> allPeriods { get; set; }
        public List<Path_Types> allPath_Types { get; set; }
        public List<Paths> allPaths { get; set; }
    }
}
