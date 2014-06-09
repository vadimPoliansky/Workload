using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkloadTest.Models;

namespace WorkloadTest.ViewModels
{
    public class IndexViewModel
    {
        public List<CoEs> allCoEs { get; set; }
        public List<Analysts> allAnalysts { get; set; }
        public List<Tasks> allTasks { get; set; }
    }
}