using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkloadTest.Helper
{
    public class DateMethods
    {
        public static DateTime DateAdd(DateTime? Start_Date, string Period, double Frequency, int instance)
        {
            DateTime Task_Date;

            switch (Period){
                case "h":
                    Task_Date = Start_Date.Value.AddHours(Frequency * instance);
                    break;
                case "d":
                    Task_Date = Start_Date.Value.AddDays(Frequency * instance);
                    break;
                case "ww":
                    Task_Date = Start_Date.Value.AddDays(Frequency * instance * 7);
                    break;
                case "m":
                    Task_Date = Start_Date.Value.AddMonths((int)Frequency * instance);
                    break;
                case "q":
                    Task_Date = Start_Date.Value.AddMonths((int)Frequency * instance * 3);
                    break;
                case "yyyy":
                    Task_Date = Start_Date.Value.AddYears((int)Frequency * instance);
                    break;
                case "w":
                    Task_Date = Start_Date.Value.AddMonths(instance);
                    Task_Date = DateAdd(Task_Date, "d", -Task_Date.Day, 1);
                    for (var i = 1; i <= Frequency; i++) {
                        if (Task_Date.AddDays(1).DayOfWeek == DayOfWeek.Saturday || Task_Date.AddDays(1).DayOfWeek == DayOfWeek.Sunday)
                        {
                            Task_Date = Task_Date.AddDays(1);
                            i -= 1;
                        } else {
                            Task_Date = Task_Date.AddDays(1);
                        }
                    }
                    break;
                default:
                    Task_Date = Start_Date.Value;
                    break;
            }
            return Task_Date;
        }
    }
}