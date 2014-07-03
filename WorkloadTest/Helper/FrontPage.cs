using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Novacode;
using System.Diagnostics;
using WorkloadTest.Helper;
using WorkloadTest.ViewModels;
using WorkloadTest.Models;

namespace WorkloadTest.Helper
{
    public class FrontPage
    {
        public static void CreateFrontPage(InstanceListViewModel instanceList, string analystName, List<Paths> allPaths, List<Path_Types> allPathTypes)
        {
            var filePathTemplate = HttpContext.Current.Server.MapPath("~/App_Data/frontPageTemplate.docx");
            var filePathFrontPage = HttpContext.Current.Server.MapPath("~/App_Data/frontPage.docx");

            var docTemplate = DocX.Load(filePathTemplate);

            using (var combined = DocX.Load(filePathFrontPage))
            {
                foreach (var instance in instanceList.allInstances)
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
                    foreach (var otherPath in allPaths.Where(x=>x.Task_ID == instance.Task_ID && x.Path_Type_ID == 1))
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
                    combined.ReplaceText("%Request_Date%", instance.Request_Date != null ? instance.Request_Date.Value.ToShortDateString() : " ");
                    combined.ReplaceText("%Task_Date%", instance.Task_Date != null ? instance.Task_Date.Value.ToShortDateString() : " ");
                    combined.ReplaceText("%Purpose%", instance.Purpose ?? " ");
                    combined.ReplaceText("%Comment%", instance.Comment ?? " ");
                    combined.ReplaceText("%Analyst%", analystName);
                    combined.ReplaceText("%Paths%", pathString ?? " ");
                }
                combined.Save();
            }

            Process.Start("WINWORD.EXE", filePathFrontPage);
        }

        public string ReplaceFirst(string text, string search, string replace)
        {
          int pos = text.IndexOf(search);
          if (pos < 0)
          {
            return text;
          }
          return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

    }
}