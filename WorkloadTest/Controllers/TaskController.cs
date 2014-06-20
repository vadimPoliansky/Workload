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

        public ActionResult TaskList(int? coeID, int? analystID)
        {
            List<InstanceViewModel> allInstances = new List<InstanceViewModel>();
            InstanceListViewModel viewModel = new InstanceListViewModel();
            
            List<Periods> periods = new List<Periods>();
            List<Exceptions> exceptions = new List<Exceptions>();
            List<Workload_Units> workload_units = new List<Workload_Units>();
            periods = db.Periods.ToList();
            exceptions = db.Exceptions.ToList();
            workload_units = db.Workload_Units.ToList();

            Exceptions exception = new Exceptions();

            List<Tasks> allTasks = db.Tasks.Where(x => x.Saved.Value).ToList();
            if (coeID.HasValue)
            {
                if (coeID != 5)
                {
                    allTasks = allTasks.Where(x => x.CoE_ID == coeID).ToList();
                }
            }
            if (analystID.HasValue)
            {
                allTasks = allTasks.Where(x => x.Analyst_ID == analystID).ToList();
            }


            foreach (var task in allTasks)
            {
                for (var i = 0; i <= task.Count; i++)
                {
                    exception = exceptions.FirstOrDefault(x=>x.Task_ID == task.Task_ID && x.Instance_ID == i);
                    bool canceled;
                    if (exception != null)  {
                        canceled = exception.Canceled;
                    }   else   {
                        canceled = false;
                    }
                    if (canceled == false)
                    {
                        DateTime? taskDate;
                        double? workload;
                        string workloadUnit;
                        if (exception != null && exception.Date != null)
                        {
                            taskDate = exception.Date;
                            if (!task.Routine) { i = task.Count.Value + 1; }
                        }
                        else if (!task.Routine)
                        {
                            taskDate = task.Start_Date;
                            i = task.Count.Value + 1;
                        }
                        else
                        {
                            taskDate = task.Start_Date != null ? Helper.DateMethods.DateAdd(task.Start_Date,
                                                                    task.Period_ID == null ? null : periods.FirstOrDefault(x => x.Period_ID == task.Period_ID).Period,
                                                                    task.Frequency.Value,
                                                                    i) :
                                                                (DateTime?)null;

                        }
                        if (exception != null && exception.Workload != null)
                        {
                            workload = exception.Workload;
                            if (exception.Workload_Unit_ID != null ) {
                                workloadUnit = workload_units.FirstOrDefault(x => x.Workload_Unit_ID == exception.Workload_Unit_ID).Workload_Unit;
                            } else {
                                workloadUnit = "d";
                            }
                        } else {
                            workload = task.Workload == null ? 0 : task.Workload;
                            workloadUnit = workload_units.FirstOrDefault(x => x.Workload_Unit_ID == task.Workload_Unit_ID).Workload_Unit;
                        }
                        DateTime? taskDateFrom = null;
                        if (workload != 0)
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
                        InstanceViewModel instance = new InstanceViewModel
                        {
                            Task_ID = task.Task_ID,
                            Routine = task.Routine,
                            Priority = task.Priority,
                            CoE_ID = task.CoE_ID,
                            Analyst_ID = task.Analyst_ID,
                            Description = task.Description,
                            Purpose = task.Purpose,
                            Requestor = task.Requestor,
                            Workload = task.Workload,
                            Workload_Unit_ID = task.Workload_Unit_ID,
                            Comment = task.Comment,
                            Request_Date = task.Request_Date,
                            Task_Date = taskDate,
                            Task_Date_From = taskDateFrom,
                            User_Added = task.User_Added,
                            Date_Added = task.Date_Added,
                            Instance = task.Routine ? i : 0,
                            Instance_Comment = exception != null? exception.Comment : null,
                            Exception_ID = exception != null ? exception.Exception_ID: 0,
                        };
                        allInstances.Add(instance);
                    }
                }
            }

            viewModel.allInstances = allInstances.ToList();
            viewModel.allCoEs = db.CoEs.ToList();
            viewModel.allAnalysts = db.Analysts.ToList();
            viewModel.allWorkload_Units = db.Workload_Units.ToList();
            
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult editTable(string taskID)
        {
            var viewModelItems = db.Tasks.Where(x => x.Saved == true).ToList();
            var viewModelTasks = viewModelItems.Select(x => new TaskViewModel
            {
                Task_ID= x.Task_ID,
                Routine= x.Routine,
                Priority= x.Priority,

                CoE_ID= x.CoE_ID,
                Analyst_ID= x.Analyst_ID,

                Description= x.Description,
                Purpose = x.Purpose,
                Requestor= x.Requestor,
                Workload= x.Workload,
                Workload_Unit_ID= x.Workload_Unit_ID,
                Comment= x.Comment,

                Data_Source= x.Data_Source,
                Report_Location= x.Report_Location,

                Start_Date = x.Start_Date != null ? x.Start_Date.Value.ToString("yyyy-MM-dd") : "",
                Request_Date = x.Request_Date != null ? x.Request_Date.Value.ToString("yyyy-MM-dd"): "",

                Count= x.Count,
                Frequency= x.Frequency,
                Period_ID= x.Period_ID,

                User_Added= x.User_Added,
                Date_Added= x.Date_Added != null ? x.Date_Added.Value.ToString("yyyy-MM-dd"): "",

                Saved= x.Saved,

                Analyst= x.Analyst != null ? x.Analyst.First_Name : "",
                CoE= x.CoE != null? x.CoE.CoE : "",
                Period = x.Period != null? x.Period.Period_Name : "",
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

        [HttpPost]
        public JsonResult editTable(IList<TaskViewModel> taskChange)
        {
            var taskID = taskChange[0].Task_ID;
            string analyst = taskChange[0].Analyst;
            string coe = taskChange[0].CoE;
            string period = taskChange[0].Period;
            int? analystID = null;
            int? coeID = null;
            int? periodID = null;
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
                periodID = db.Periods.Any(x => x.Period == period) ? db.Periods.FirstOrDefault(x => x.Period == period).Period_ID : (int?)null;
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
                        Workload_Unit_ID = taskChange[0].Workload_Unit_ID,
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
            if (db.Exceptions.Any(x => x.Task_ID == exception.Task_ID && x.Instance_ID == exception.Instance_ID))
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
                if (ModelState.IsValid)
                {
                    db.Exceptions.Add(exception);
                    db.SaveChanges();
                    viewModel = db.Exceptions.FirstOrDefault(x => x.Exception_ID == exception.Exception_ID);
                }
            }

            ModelState.Clear();

            return Json(viewModel, JsonRequestBehavior.AllowGet);

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

                    combined.ReplaceText("%Description%", instance.Description ?? " ");
                    combined.ReplaceText("%Requestor%", instance.Requestor ?? " ");
                    combined.ReplaceText("%Request_Date%", instance.Request_Date != null ? instance.Request_Date.ToString() : " ");
                    combined.ReplaceText("%Task_Date%", instance.Task_Date != null ? instance.Task_Date.ToString() : " ");
                    combined.ReplaceText("%Purpose%", instance.Purpose ?? " ");
                    combined.ReplaceText("%Comment%", instance.Comment ?? " ");
                    combined.ReplaceText("%Analyst%", analystName);
                    combined.ReplaceText("%Paths%", pathString ?? " ");
                }
                MemoryStream ms = new MemoryStream();
                combined.SaveAs(ms);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment; filename=\"" + "DFGFDG" + ".doc\"");
                Response.ContentType = "application/msword";

                //ms.WriteTo(Response.OutputStream);
                //Response.End();
                //return Json(Response, JsonRequestBehavior.AllowGet);
                var filename = "coverPage" + DateTime.Now.ToString("hmmss") + ".docx";
                var filenamePath = Server.MapPath("~/Docs") + "/" + filename;
                combined.SaveAs(filenamePath);

                return Json("/../../Docs1/" + filename, JsonRequestBehavior.AllowGet);
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
            return RedirectToAction("Index");
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