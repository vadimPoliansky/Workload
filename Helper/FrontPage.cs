using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Novacode;
using System.Diagnostics;
using WorkloadTest.Helper;
using WorkloadTest.ViewModels;

namespace WorkloadTest.Helper
{
    public class FrontPage
    {
        public static void CreateFrontPage(InstanceListViewModel instanceList)
        {
            var filePathTemplate = HttpContext.Current.Server.MapPath("~/App_Data/frontPageTemplate.docx");
            var filePathFrontPage = HttpContext.Current.Server.MapPath("~/App_Data/frontPage.docx");

            var docTemplate = DocX.Load(filePathTemplate);

            using (var combined = DocX.Create(filePathFrontPage))
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
                    combined.ReplaceText("%Description%", instance.Description.ToString());
                    combined.ReplaceText("%Requestor%", instance.Requestor.ToString());
                    combined.ReplaceText("%Request_Date%", instance.Request_Date.ToString());
                    combined.ReplaceText("%Task_Date%", instance.Task_Date.ToString());
                    combined.ReplaceText("%Purpose%", instance.Purpose.ToString());
                    combined.ReplaceText("%Comment%", instance.Comment.ToString());
                    combined.ReplaceText("%Analyst%", "SADFDSAFDSA");
                }
                combined.Save();
            }

            Process.Start("WINWORD.EXE", filePathFrontPage);
        }
    }
}