using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkloadTest.Models;
using WorkloadTest.ViewModels;
using WorkloadTest.Helper;
using Novacode;
using System.Diagnostics;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;

namespace WorkloadTest.Controllers
{
    public class TaskController : Controller
    {
        private TaskDBContext db = new TaskDBContext();

        //
        // GET: /Task/

        public ActionResult Index()
        {
            var viewModel = new IndexViewModel
            {
                allCoEs = db.CoEs.ToList(),
                allAnalysts = db.Analysts.ToList(),
                allTasks = db.Tasks.ToList(),
            };
            return View(viewModel);
        }

        public ActionResult TaskList(int? coeID, int? analystID, DateTime? date, bool? getTasks, bool? toExcel)
        {
            List<InstanceViewModel> allInstances = new List<InstanceViewModel>();
            List<CalendarViewModel> allInstancesCalendar = new List<CalendarViewModel>();
            InstanceListViewModel viewModel = new InstanceListViewModel();
            
            List<Periods> periods = new List<Periods>();
            List<Exceptions> exceptions = new List<Exceptions>();
            List<Workload_Units> workload_units = new List<Workload_Units>();
            periods = db.Periods.ToList();
            exceptions = db.Exceptions.ToList();
            workload_units = db.Workload_Units.ToList();

            Exceptions exception = new Exceptions();

            List<Tasks> allTasks = db.Tasks.ToList();
            if (coeID.HasValue)
            {
                //if (coeID != 5)
                //{
                    allTasks = allTasks.Where(x => x.CoE_ID == coeID).ToList();
                //}
            }
            if (analystID.HasValue)
            {
                allTasks = allTasks.Where(x => x.Analyst_ID == analystID).ToList();
            }


            foreach (var task in allTasks)
            {
                if (task.Count == null || task.Count == 0) { task.Count = 1; }
                for (var i = 0; i <= task.Count; i++)
                {
                    exception = exceptions.FirstOrDefault(x => x.Task_ID == task.Task_ID && x.Instance_ID == i);
                    bool canceled;
                    if (exception != null)
                    {
                        canceled = exception.Canceled;
                    }
                    else
                    {
                        canceled = false;
                    }
                    if (canceled == false)
                    {
                        DateTime? taskDate;
                        string taskDateTime;
                        double? workload;
                        string workloadUnit;
                        bool resch = false;
                        if (exception != null && exception.Date != null && task.Routine)
                        {
                            taskDate = exception.Date;
                            taskDateTime = exception.Time_Of_Day_ID != null ? exception.Time_Of_Day.Time_Of_Day_Abbr : null;
                            resch = true;
                            if (!task.Routine) { i = task.Count.Value + 1; }
                        }
                        else if (!task.Routine)
                        {
                            taskDate = task.Start_Date;
                            taskDateTime = task.Time_Of_Day_ID != null ? task.Time_Of_Day.Time_Of_Day_Abbr : null;
                            i = task.Count.Value + 1;
                        }
                        else
                        {
                            taskDate = task.Start_Date != null ? Helper.DateMethods.DateAdd(task.Start_Date,
                                                                    task.Period_ID == null ? null : periods.FirstOrDefault(x => x.Period_ID == task.Period_ID).Period,
                                                                    task.Frequency.Value,
                                                                    i) :
                                                                (DateTime?)null;
                            taskDateTime = task.Time_Of_Day_ID != null ? task.Time_Of_Day.Time_Of_Day_Abbr : null;

                        }
                        if (exception != null && exception.Workload != null)
                        {
                            workload = exception.Workload;
                            if (exception.Workload_Unit_ID != null)
                            {
                                workloadUnit = workload_units.FirstOrDefault(x => x.Workload_Unit_ID == exception.Workload_Unit_ID).Workload_Unit;
                            }
                            else
                            {
                                workloadUnit = "d";
                            }
                        }
                        else
                        {
                            workload = task.Workload == null ? 0 : task.Workload;
                            workloadUnit = task.Workload_Unit_ID == null ? "" : workload_units.FirstOrDefault(x => x.Workload_Unit_ID == task.Workload_Unit_ID).Workload_Unit;
                        }
                        DateTime? taskDateFrom = null;

                        if (workload != 0 && workloadUnit != "h")
                        {
                            taskDateFrom = taskDate != null ? Helper.DateMethods.DateAdd(taskDate,
                                                                        workloadUnit,
                                                                        workload != null ? -(workload.Value - 1) : 0,
                                                                        1) :
                                                                        (DateTime?)null;
                        }
                        else
                        {
                            taskDateFrom = taskDate;
                        }
                        if (Request.IsAjaxRequest())
                        {
                            if (taskDate != null)
                            {
                                string endTime;
                                switch (taskDateTime)
                                {
                                    case "Morn":
                                        endTime = "12:30:00 AM";
                                        break;
                                    case "Aftr":
                                        endTime = "01:00:00  AM";
                                        break;
                                    case "End":
                                    default:
                                        endTime = "01:30:00  AM";
                                        break;

                                }
                                CalendarViewModel instance = new CalendarViewModel
                                {
                                    title = (task.CoE != null ? task.CoE.CoE_Abbr : "") + " - " + task.Description,
                                    start = taskDateFrom.Value.Year + " - " + taskDateFrom.Value.Month + " - " + taskDateFrom.Value.Day + " 12:00:00 AM",
                                    end = taskDate.Value.Year + " - " + taskDate.Value.Month + " - " + taskDate.Value.Day + " " + endTime,
                                    task_id = task.Task_ID.ToString(),
                                    instance_id = task.Routine ? i.ToString() : "0",
                                    analyst_id = @task.Analyst_ID.ToString(),
                                    coe_id = task.CoE_ID.ToString(),
                                    //color = eventColorsCoE.Color_@task.CoE_ID,
                                    priority = task.Priority.ToString(),
                                };
                                allInstancesCalendar.Add(instance);
                            }
                        }
                        else if (getTasks.HasValue && getTasks.Value)
                        {
                            InstanceViewModel instance = new InstanceViewModel
                            {
                                Task_ID = task.Task_ID,
                                Routine = task.Routine,
                                Priority = task.Priority,
                                CoE_ID = task.CoE_ID,
                                CoE = task.CoE,
                                Analyst_ID = task.Analyst_ID,
                                Analyst = task.Analyst,
                                Description = task.Description,
                                Purpose = task.Purpose,
                                Requestor = task.Requestor,
                                Workload = task.Workload,
                                Workload_Unit_ID = task.Workload_Unit_ID,
                                Workload_Unit = task.Workload_Unit,

                                Comment = task.Comment,
                                Request_Date = task.Request_Date,

                                Task_Date = taskDate,
                                Task_Date_Time = taskDateTime,

                                Task_Date_From = taskDateFrom,

                                User_Added = task.User_Added,
                                Date_Added = task.Date_Added,
                                Instance = task.Routine ? i : 0,
                                Instance_Comment = exception != null ? exception.Comment : null,
                                Exception_ID = exception != null ? exception.Exception_ID : 0,

                                Rescheduled = resch,
                            };
                            allInstances.Add(instance);
                        }
                    }
                }
            }

            /*if (date.HasValue) {
                var instancesAtDate = allInstances.Where(x => x.Task_Date == date.Value).ToList();
                return Json(instancesAtDate, JsonRequestBehavior.AllowGet);
            } else {
                viewModel.allInstances = allInstances.ToList();
            }*/

            if (Request.IsAjaxRequest())
            {
                return Json(allInstancesCalendar, JsonRequestBehavior.AllowGet);
            }
            else
            {
                viewModel.allInstances = allInstances.ToList();
                viewModel.allCoEs = db.CoEs.ToList();
                viewModel.allAnalysts = db.Analysts.ToList();
                viewModel.allWorkload_Units = db.Workload_Units.ToList();
                viewModel.allTime_Of_Days = db.Time_Of_Days.ToList();
            }

            if (toExcel.HasValue && toExcel.Value)
            {
                string path = HttpContext.Server.MapPath("~/App_Data/excelTemp.xlsx");

                var wb = new XLWorkbook(path);

                
                string[] monthNames =
                    System.Globalization.CultureInfo.CurrentCulture
                    .DateTimeFormat.MonthGenitiveNames;


                var ws = wb.Worksheet("byCoEwAdhoc");
                var startCol = 1;
                var currRow = 1;
                var currCol = startCol;
                var colHeaderStart = 3;
                ws.Cell(currRow, colHeaderStart).Value = "Routine";
                ws.Cell(currRow, colHeaderStart + 1).Value = "Adhoc";
                currRow++;

                for (var month = 1; month <= 12; month++) 
                {
                    var coeCount = viewModel.allCoEs.Count();
                    ws.Cell(currRow, currCol).Value = monthNames[month - 1];

                    ws.Range(ws.Cell(currRow, currCol).Address, ws.Cell(currRow + coeCount - 1, currCol).Address).Merge();

                    currCol++;

                    foreach (var coe in viewModel.allCoEs)
                    {
                        var sumRoutine = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine == true).Where(x=> (x.CoE_ID ?? 0) == coe.CoE_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );
                        var sumAdhoc = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine != true).Where(x => (x.CoE_ID ?? 0) == coe.CoE_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );


                        ws.Cell(currRow, currCol).Value = coe.CoE_Abbr;
                        currCol++;
                        ws.Cell(currRow, currCol).Value = sumRoutine;
                        currCol++;
                        ws.Cell(currRow, currCol).Value = sumAdhoc;
                        currCol++;

                        currCol = startCol + 1;
                        currRow++;
                    }
                    currCol = startCol;
                    currRow++;
                }

                ws = wb.Worksheet("byCoE");
                currRow = 1;
                currCol = startCol;
                var headerRow = currRow;
                currRow++;

                for (var month = 1; month <= 12; month++)
                {
                    var coeCount = viewModel.allCoEs.Count();
                    ws.Cell(currRow, 1).Value = monthNames[month - 1];
                    currCol++;
                    foreach (var coe in viewModel.allCoEs)
                    {
                        var sumRoutine = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine == true).Where(x => (x.CoE_ID ?? 0) == coe.CoE_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );
                        var sumAdhoc = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine != true).Where(x => (x.CoE_ID ?? 0) == coe.CoE_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );

                        ws.Cell(currRow, currCol).Value = sumRoutine;
                        ws.Cell(headerRow, currCol).Value = coe.CoE_Abbr;
                        currCol++;
                    }
                    currCol = startCol;
                    currRow++;
                }

                ws = wb.Worksheet("byAnalystwAdhoc");
                startCol = 1;
                currRow = 1;
                currCol = startCol;
                colHeaderStart = 3;
                ws.Cell(currRow, colHeaderStart).Value = "Routine";
                ws.Cell(currRow, colHeaderStart + 1).Value = "Adhoc";
                currRow++;

                for (var month = 1; month <= 12; month++)
                {
                    var analystCount = viewModel.allAnalysts.Count();
                    ws.Cell(currRow, currCol).Value = monthNames[month - 1];

                    ws.Range(ws.Cell(currRow, currCol).Address, ws.Cell(currRow + analystCount - 1, currCol).Address).Merge();

                    currCol++;

                    foreach (var analyst in viewModel.allAnalysts)
                    {
                        var sumRoutine = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine == true).Where(x => (x.Analyst_ID ?? 0) == analyst.Analyst_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );
                        var sumAdhoc = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine != true).Where(x => (x.Analyst_ID ?? 0) == analyst.Analyst_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );


                        ws.Cell(currRow, currCol).Value = analyst.First_Name;
                        currCol++;
                        ws.Cell(currRow, currCol).Value = sumRoutine;
                        currCol++;
                        ws.Cell(currRow, currCol).Value = sumAdhoc;
                        currCol++;

                        currCol = startCol + 1;
                        currRow++;
                    }
                    currCol = startCol;
                    currRow++;
                }

                ws = wb.Worksheet("byAnalyst");
                currRow = 1;
                currCol = startCol;
                headerRow = currRow;
                currRow++;

                for (var month = 1; month <= 12; month++)
                {
                    var analystCount = viewModel.allAnalysts.Count();
                    ws.Cell(currRow, 1).Value = monthNames[month - 1];
                    currCol++;
                    foreach (var analyst in viewModel.allAnalysts)
                    {
                        var sumRoutine = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine == true).Where(x => (x.Analyst_ID ?? 0) == analyst.Analyst_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );
                        var sumAdhoc = allInstances.Where(x => x.Task_Date.HasValue ? x.Task_Date.Value.Month == month : false).Where(x => x.Routine != true).Where(x => (x.Analyst_ID ?? 0) == analyst.Analyst_ID).Sum(
                            x => x.Workload * x.Workload_Unit.Value
                        );

                        ws.Cell(currRow, currCol).Value = sumRoutine;
                        ws.Cell(headerRow, currCol).Value = analyst.First_Name;
                        currCol++;
                    }
                    currCol = startCol;
                    currRow++;
                }

                ws = wb.Worksheet("Raw");
                currRow = 1;
                startCol = 1;
                currCol = startCol;
                headerRow = currRow;
                currRow++;

                foreach (var task in allInstances)
                {
                    ws.Cell(currRow, currCol).Value = task.Task_ID;
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Task_Date.HasValue ? task.Task_Date.Value : (DateTime?)null;
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Priority == true ? "Priority" : "Not Priority";
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Routine == true ? "Routine" : "Ad-hoc";
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.CoE != null ? task.CoE.CoE_Abbr : "";
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Analyst != null ? task.Analyst.First_Name : "";
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Description;
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Purpose;
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Rescheduled == true ? "Rescheduled" : "";
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Workload;
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Workload_Unit != null ? task.Workload_Unit.Workload_Unit_Name : "";
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Workload_Unit != null ? task.Workload_Unit.Value : 0;
                    currCol++;
                    ws.Cell(currRow, currCol).Value = task.Workload_Unit != null ? task.Workload * task.Workload_Unit.Value : 0;
                    currCol++;

                    currCol = startCol;
                    currRow++;
                }

                HttpResponse httpResponse = this.HttpContext.ApplicationInstance.Context.Response;
                httpResponse.Clear();
                httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                httpResponse.AddHeader("content-disposition", "attachment;filename=\"workloadStats.xlsx\"");
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    wb.SaveAs(memoryStream);
                    memoryStream.WriteTo(httpResponse.OutputStream);
                    memoryStream.Close();
                }

                httpResponse.End();

                return View(viewModel);
            }
            else
            {
                return View(viewModel);
            }
        }

        public ActionResult reports()
        {
            return(TaskList(null, null, null, true, true));
        }

        [HttpGet]
        public ActionResult editTable(string taskID)
        {
            if (taskID != "")
            {
                var viewModelItems = db.Tasks.ToList();
                var viewModelTasks = viewModelItems.Select(x => new TaskViewModel
                {
                    Task_ID = x.Task_ID,
                    Routine = x.Routine,
                    Priority = x.Priority,

                    CoE_ID = x.CoE_ID,
                    Analyst_ID = x.Analyst_ID,

                    Description = x.Description,
                    Purpose = x.Purpose,
                    Requestor = x.Requestor,
                    Workload = x.Workload,
                    Workload_Unit_ID = x.Workload_Unit_ID,
                    Workload_Unit = x.Workload_Unit != null ? x.Workload_Unit.Workload_Unit_Name : "",
                    Comment = x.Comment,

                    Data_Source = x.Data_Source,
                    Report_Location = x.Report_Location,

                    Start_Date = x.Start_Date != null ? x.Start_Date.Value.ToString("yyyy-MM-dd") : "",
                    Request_Date = x.Request_Date != null ? x.Request_Date.Value.ToString("yyyy-MM-dd") : "",

                    Count = x.Count,
                    Frequency = x.Frequency,
                    Period_ID = x.Period_ID,

                    User_Added = x.User_Added,
                    Date_Added = x.Date_Added != null ? x.Date_Added.Value.ToString("yyyy-MM-dd") : "",

                    Saved = x.Saved,

                    Analyst = x.Analyst != null ? x.Analyst.First_Name : "",
                    CoE = x.CoE != null ? x.CoE.CoE : "",
                    Period = x.Period != null ? x.Period.Period_Name : "",
                }).ToList();
                var viewModel = new TaskListViewModel
                {
                    allTasks = viewModelTasks,
                    allCoEs = db.CoEs.ToList(),
                    allAnalysts = db.Analysts.ToList(),
                };
                if (Request.IsAjaxRequest())
                {
                    return Json(viewModel.allTasks.Where(x => x.Task_ID.ToString().Contains(taskID == null ? "" : taskID)), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View(viewModel);
                }
            }
            else
            {
                var newTask = new Tasks();
                db.Tasks.Add(newTask);
                db.SaveChanges();

                return Json(newTask.Task_ID, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult editTable(IList<TaskViewModel> taskChange)
        {
            var taskID = taskChange[0].Task_ID;
            string analyst = taskChange[0].Analyst;
            string coe = taskChange[0].CoE;
            string period = taskChange[0].Period;
            string workload = taskChange[0].Workload_Unit;
            int? analystID = null;
            int? coeID = null;
            int? periodID = null;
            int? workloadUnitID = null;
            if (taskChange[0].Analyst != null && taskChange[0].Analyst != "")
            {
                analystID = db.Analysts.Any(x => x.First_Name == analyst) ?  db.Analysts.FirstOrDefault(x => x.First_Name == analyst).Analyst_ID : (int?)null;
            }

            if (taskChange[0].CoE != null && taskChange[0].CoE != "")
            {
                coeID = db.CoEs.Any(x => x.CoE == coe) ? db.CoEs.FirstOrDefault(x => x.CoE == coe).CoE_ID : (int?)null;
            }
            if (taskChange[0].Period != null && taskChange[0].Period != "")
            {
                periodID = db.Periods.Any(x => x.Period_Name == period) ? db.Periods.FirstOrDefault(x => x.Period_Name == period).Period_ID : (int?)null;
            }
            if (taskChange[0].Workload_Unit != null && taskChange[0].Workload_Unit != "")
            {
                workloadUnitID = db.Workload_Units.Any(x => x.Workload_Unit_Name == workload) ? db.Workload_Units.FirstOrDefault(x => x.Workload_Unit_Name == workload).Workload_Unit_ID : (int?)null;
            }
            if (db.Tasks.Any(x => x.Task_ID == taskID))
            {
                if (ModelState.IsValid)
                {
                    var newTask = new Tasks
                    {
                        Task_ID = taskChange[0].Task_ID,
                        Routine = taskChange[0].Routine,
                        Priority = taskChange[0].Priority,

                        //CoE_ID= taskChange[0].CoE_ID,
                        //Analyst_ID= taskChange[0].Analyst_ID,

                        Description = taskChange[0].Description,
                        Purpose = taskChange[0].Purpose,
                        Requestor = taskChange[0].Requestor,
                        Workload = taskChange[0].Workload,
                        Comment = taskChange[0].Comment,

                        Data_Source = taskChange[0].Data_Source,
                        Report_Location = taskChange[0].Report_Location,

                        Start_Date = taskChange[0].Start_Date != null ? DateTime.ParseExact(taskChange[0].Start_Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : (DateTime?)null,
                        Request_Date = taskChange[0].Request_Date != null ? DateTime.ParseExact(taskChange[0].Request_Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : (DateTime?)null,

                        Count = taskChange[0].Count,
                        Frequency = taskChange[0].Frequency,
                        Period_ID = periodID ?? null,

                        User_Added = taskChange[0].User_Added,
                        Date_Added = taskChange[0].Date_Added != null ? DateTime.ParseExact(taskChange[0].Date_Added, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : (DateTime?)null,

                        Saved = taskChange[0].Saved,

                        Analyst_ID = analystID ?? null,
                        CoE_ID = coeID ?? null,
                        Workload_Unit_ID = workloadUnitID ?? null,

                    };

                    db.Entry(newTask).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json("",JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var newTask = new Tasks();
                    db.Tasks.Add(newTask);
                    db.SaveChanges();
                    return Json(newTask.Task_ID, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
           }

        }

        [HttpPost]
        public JsonResult addTask()
        {
            var newTask = new Tasks();
            newTask.Saved = true;
            db.Tasks.Add(newTask);
            db.SaveChanges();
            return Json(newTask, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void deleteTask(int taskID)
        {
            Tasks tasks = db.Tasks.FirstOrDefault(x=>x.Task_ID == taskID);
            db.Tasks.Remove(tasks);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult getCoEs()
        {
            return Json(db.CoEs.Select(x=>x.CoE.ToLower()).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getPeriods()
        {
            return Json(db.Periods.Select(x => x.Period_Name.ToLower()).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getAnalysts()
        {
            return Json(db.Analysts.Select(x => x.First_Name.ToLower()).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getWorkload_Units()
        {
            return Json(db.Workload_Units.Select(x => x.Workload_Unit_Name.ToLower()).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult instances(int Task_ID, DateTime Start_Date, int Frequency, int Period_ID, int Count)
        {
            List<RecurrenceViewModel> viewModel = new List<RecurrenceViewModel>();

            List<Exceptions> exceptions = new List<Exceptions>();
            List<Periods> periods = new List<Periods>();
            periods = db.Periods.ToList();
            exceptions = db.Exceptions.ToList();
            for (var i = 0; i <= Count; i++)
            {
                bool canceled = false;
                string comment = null;
                DateTime? newDate = null;
                int? exceptionID = null;
                if (Task_ID != 0)
                {
                    var exception = exceptions.FirstOrDefault(x => x.Task_ID == Task_ID && x.Instance_ID == i);
                    if (exception != null)
                    {
                        exceptionID = exception.Exception_ID;
                        canceled = exception.Canceled;
                        comment = exception.Comment;
                        newDate = exception.Date;
                    }
                }
                RecurrenceViewModel recurrence = new RecurrenceViewModel
                {
                    Exception_ID = exceptionID,
                    Instance_ID = i,
                    Canceled = canceled,
                    Instance_Comment = comment,
                    Task_Date = Helper.DateMethods.DateAdd(Start_Date,
                                                    periods.FirstOrDefault(x => x.Period_ID == Period_ID).Period,
                                                    Frequency,
                                                    i),
                    New_Task_Date = newDate,
                };
                viewModel.Add(recurrence);
            }

            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult drop(int Task_ID_Filter, int Instance_ID_Filter)
        {
            var viewModel = db.Exceptions.FirstOrDefault(x => x.Task_ID == Task_ID_Filter && x.Instance_ID == Instance_ID_Filter);
            if (viewModel == null){
                viewModel = new Exceptions();
                }
            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult drop(IList<Exceptions> change)
        {
            Exceptions viewModel = new Exceptions();

            var exception = change[0];
            var routine = db.Tasks.Any(x => x.Task_ID == exception.Task_ID) ? db.Tasks.FirstOrDefault(x=>x.Task_ID == exception.Task_ID).Routine : false;
            if (db.Exceptions.Any(x => x.Task_ID == exception.Task_ID && x.Instance_ID == exception.Instance_ID))
            {
                if (routine)
                {
                    if (ModelState.IsValid)
                    {
                        //exception.Exception_ID = db.Exceptions.FirstOrDefault(x => x.Task_ID == exception.Task_ID && x.Instance_ID == exception.Instance_ID).Exception_ID;
                        db.Entry(exception).State = EntityState.Modified;
                        db.SaveChanges();
                        viewModel = db.Exceptions.FirstOrDefault(x => x.Exception_ID == exception.Exception_ID);
                    }
                }
                else
                {
                    var task = db.Tasks.FirstOrDefault(x => x.Task_ID == exception.Task_ID);
                    task.Start_Date = exception.Date;
                    db.Entry(task).Property(x => x.Start_Date).IsModified = true;
                    db.SaveChanges();
                }
            }
            else
            {
                if (routine)
                {
                    if (ModelState.IsValid)
                    {
                        db.Exceptions.Add(exception);
                        db.SaveChanges();
                        viewModel = db.Exceptions.FirstOrDefault(x => x.Exception_ID == exception.Exception_ID);
                    }
                }
                else
                {
                    var task = db.Tasks.FirstOrDefault(x => x.Task_ID == exception.Task_ID);
                    task.Start_Date = exception.Date;
                    db.Entry(task).Property(x => x.Start_Date).IsModified = true;
                    db.SaveChanges();
                }
            }

            ModelState.Clear();

            return Json(viewModel, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public void deleteDrop(IList<Exceptions> delete)
        {
            var deleteID = delete[0].Exception_ID;
            var deleteException = db.Exceptions.FirstOrDefault(x => x.Exception_ID == deleteID);
            if (deleteException != null)
            {
                db.Exceptions.Remove(deleteException);
                db.SaveChanges();
            }
        }

        public ActionResult EditException(int Task_ID, int Instance_ID)
        {
            List<Exceptions> exceptions = new List<Exceptions>();
            exceptions = db.Exceptions.ToList();

            var instance = new InstanceViewModel
            {
                Task_ID = Task_ID,
                Instance = Instance_ID,
            };

            return View(instance);
        }

        //
        // GET: /Task/Details/5

        public ActionResult Details(int? id = null)
        {
            Tasks tasks = db.Tasks.Find(id);
            if (tasks == null)
            {
                return HttpNotFound();
            }
            return View(tasks);
        }

        [HttpGet]
        public ActionResult Create(int? id)
        {
            var task = new Tasks();
            if (id.HasValue)
            {
                task = db.Tasks.FirstOrDefault(x => x.Task_ID == id);
                if (task == null)
                {
                    task = db.Tasks.Create();
                    db.Tasks.Add(task);
                    db.SaveChanges();
                }
            }
            else
            {
                task = db.Tasks.Create();
                db.Tasks.Add(task);
                db.SaveChanges();
            }
            var viewModel = new CreateViewModel
            {
                
                task = task,
                allCoEs = db.CoEs.ToList(),
                allWorkload_Units = db.Workload_Units.ToList(),
                allAnalysts = db.Analysts.ToList(),
                allPeriods = db.Periods.ToList(),
                allPath_Types = db.Path_Types.ToList(),
                allPaths = db.Paths.ToList(),
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Create(IList<Tasks> tasks)
        {
            var task = tasks[0];
            if (ModelState.IsValid)
            {
                db.Entry(task).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tasks);
        }

        public JsonResult createFrontPage(IList<InstanceListViewModel> instanceList)
        {
            var analystID = instanceList[0].allInstances.FirstOrDefault().Analyst_ID;
            var analyst = db.Analysts.FirstOrDefault(x=>x.Analyst_ID == analystID);
            var analystName = analyst.Last_Name + " " + analyst.First_Name + ", " + analyst.Position;

            var allPaths = db.Paths.ToList();
            var allPathTypes = db.Path_Types.ToList();
            //Helper.FrontPage.CreateFrontPage(instanceList[0], analystName, allPaths, allPathTypes);

            var filePathTemplate = HttpContext.Server.MapPath("~/App_Data/frontPageTemplate.docx");
            var filePathFrontPage = HttpContext.Server.MapPath("~/App_Data/frontPage.docx");

            var docTemplate = DocX.Load(filePathTemplate);

            using (var combined = DocX.Load(filePathFrontPage))
            {
                foreach (var instance in instanceList[0].allInstances)
                {
                    combined.InsertDocument(docTemplate);
                    combined.ReplaceText("%Task_ID%", instance.Task_ID.ToString());
                    if (instance.Routine)
                    {
                        combined.ReplaceText("%Routine%", "Routine");
                    }
                    else
                    {
                        combined.ReplaceText("%Routine%", "Ad Hoc");
                    }

                    var pathString = "";
                    pathString += instance.Data_Source + Environment.NewLine;
                    var i = 1;
                    foreach (var otherPath in allPaths.Where(x => x.Task_ID == instance.Task_ID && x.Path_Type_ID == 1))
                    {
                        pathString += otherPath.Location + Environment.NewLine;
                        combined.ReplaceText("%Line" + i + "%", "");
                        i++;
                    }
                    for (var j = i; j <= 13; j++)
                    {
                        combined.ReplaceText("%Line" + j + "%", " ");
                    }

                    combined.ReplaceText("%Now%", DateTime.Now.ToString("MM/dd/yyyy") ?? " ");
                    combined.ReplaceText("%Description%", instance.Description ?? " ");
                    combined.ReplaceText("%Requestor%", instance.Requestor ?? " ");
                    combined.ReplaceText("%Request_Date%", instance.Request_Date != null ? instance.Request_Date.ToString() : " ");
                    combined.ReplaceText("%Task_Date%", instance.Task_Date != null ? instance.Task_Date.ToString() : " ");
                    combined.ReplaceText("%Purpose%", instance.Purpose ?? " ");
                    combined.ReplaceText("%Comment%", instance.Comment ?? " ");
                    combined.ReplaceText("%Analyst%", analystName);
                    combined.ReplaceText("%Paths%", pathString ?? " ");
                }
                //MemoryStream ms = new MemoryStream();
                //combined.SaveAs(ms);
                //Response.Clear();
                //Response.AddHeader("content-disposition", "attachment; filename=\"" + "DFGFDG" + ".doc\"");
                //Response.ContentType = "application/msword";

                //ms.WriteTo(Response.OutputStream);
                //Response.End();
                //return Json(Response, JsonRequestBehavior.AllowGet);
                var filenamePath = HttpContext.Server.MapPath("~/DocOutput/coverPage.docx");
                combined.SaveAs(filenamePath);

                return Json("~/DocOutput/coverPage.docx", JsonRequestBehavior.AllowGet);
                //return File(Response.ToString(), "application/x-ms-excel", "test.docx");
            }

        }

        //
        // GET: /Task/Edit/5

        public JsonResult createPath(IList<Paths> paths)
        {
            var newPath = paths[0];
            db.Paths.Add(newPath);
            db.SaveChanges();

            return Json(newPath, JsonRequestBehavior.AllowGet);
        }

        public void updatePath(IList<Paths> paths)
        {
            var path = paths[0];

            if (ModelState.IsValid)
            {
                db.Entry(path).State = EntityState.Modified;
                db.SaveChanges();
            }

        }

        public ActionResult Edit(int? id = null)
        {
            Tasks tasks = db.Tasks.Find(id);
            if (tasks == null)
            {
                return HttpNotFound();
            }
            return View(tasks);
        }

        //
        // POST: /Task/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tasks tasks)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tasks).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tasks);
        }

        //
        // GET: /Task/Delete/5


        //
        // POST: /Task/Delete/5

        public ActionResult Delete(int? id)
        {
            Tasks tasks = db.Tasks.Find(id);
            db.Tasks.Remove(tasks);
            db.SaveChanges();
            var lastID = db.Tasks.Max(x => x.Task_ID);
            return RedirectToAction("create", new { id = lastID });
        }

        public ActionResult Done(int? id)
        {
            Tasks task = db.Tasks.Find(id);
            task.Saved = true;
            db.SaveChanges();
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}