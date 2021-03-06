﻿using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkloadTest.Models
{
    public class Tasks
    {
        [Key]
        public int Task_ID { get; set; }
        public bool Routine { get; set; }
        public bool Priority { get; set; }

        [Display(Name = "Center of Excellence")]
        public int? CoE_ID { get; set; }
        [Display(Name = "Analyst")]
        public int? Analyst_ID { get; set; }


        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Requestor { get; set; }
        public double? Workload { get; set; }
        [Display(Name = "Period")]
        public int? Workload_Unit_ID { get; set; }
        [Display(Name = "OQPM Analysis")]
        public string Comment { get; set; }

        [Display(Name = "Date Source")]
        public string Data_Source { get; set; }
        [Display(Name = "Report Location")]
        public string Report_Location { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? Start_Date { get; set; }
        [Display(Name = "Time Of Day")]
        public int? Time_Of_Day_ID { get; set; }
        [Display(Name = "Request Date")]
        public DateTime? Request_Date { get; set; }

        [Display(Name = "For")]
        public int? Count { get; set; }
        [Display(Name = "Every")]
        public int? Frequency { get; set; }
        [Display(Name = "Period")]
        public int? Period_ID { get; set; }

        public string User_Added { get; set; }
        public DateTime? Date_Added { get; set; }

        public bool? Saved { get; set; } 

        public virtual ICollection<Paths> Path { get; set; }
        public virtual Analysts Analyst { get; set; }
        public virtual CoEs CoE { get; set; }
        public virtual Periods Period { get; set; }
        public virtual Workload_Units Workload_Unit { get; set; }
        public virtual Time_Of_Days Time_Of_Day { get; set; }

        public Tasks()
        {
            Time_Of_Day_ID = 3;
        }
    }

    public class Exceptions
    {
        [Key]
        public int Exception_ID { get; set; }
        public int Task_ID { get; set; }
        public int Instance_ID { get; set; }
        public DateTime? Date { get; set; }
        public int? Time_Of_Day_ID { get; set; }
        public bool Canceled { get; set; }
        public string Comment { get; set; }
        public double? Workload { get; set; }
        public int? Workload_Unit_ID { get; set; }

        public virtual Time_Of_Days Time_Of_Day { get; set; }
    }

    public class Paths
    {
        [Key]
        public int Path_ID { get; set; }
        public int Path_Type_ID { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }

        public int Task_ID { get; set; }
        public virtual Path_Types Path_Type { get; set; }
    }

    public class Path_Types
    {
        [Key]
        public int Path_Type_ID { get; set; }
        public string Path_Type { get; set; }
        public int? Sort { get; set; }
    }

    public class Analysts
    {
        [Key]
        public int Analyst_ID { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Position { get; set; }
    }

    public class CoEs
    {
        [Key]
        public int CoE_ID { get; set; }
        public string CoE { get; set; }
        public string CoE_Abbr { get; set; }
    }

    public class Workload_Units
    {
        [Key]
        public int Workload_Unit_ID { get; set; }
        public string Workload_Unit { get; set; }
        public string Workload_Unit_Name { get; set; }
        public int Sort { get; set; }
        public int Value { get; set; }
    }

    public class Periods
    {
        [Key]
        public int Period_ID { get; set; }
        public string Period { get; set; }
        public string Period_Name { get; set; }
        public int Sort { get; set; }
    }

    public class Time_Of_Days
    {
        [Key]
        public int Time_Of_Day_ID { get; set; }
        public String Time_Of_Day { get; set; }
        public String Time_Of_Day_Abbr { get; set; }
        public int Sort { get; set; }
        public int Value { get; set; }
    }

    public class TaskDBContext : DbContext
    {
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<CoEs> CoEs { get; set; }
        public DbSet<Analysts> Analysts { get; set; }
        public DbSet<Exceptions> Exceptions { get; set; }
        public DbSet<Paths> Paths { get; set; }
        public DbSet<Periods> Periods { get; set; }
        public DbSet<Workload_Units> Workload_Units { get; set; }
        public DbSet<Path_Types> Path_Types { get; set; }
        public DbSet<Time_Of_Days> Time_Of_Days { get; set; }

    }

}