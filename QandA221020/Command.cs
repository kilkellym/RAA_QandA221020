#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using NLog;

#endregion

namespace QandA221020
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Views);
            collector.WhereElementIsNotElementType();

            List<View> viewsToKeep = new List<View>();
            List<View> viewsToDelete = new List<View>();

            InitLogger();

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
                Logger.Info("started transaction");

                foreach (View curView in collector)
                {
                    if (curView.Name.Contains("C_") == false)
                        viewsToDelete.Add(curView);
                    else
                        viewsToKeep.Add(curView);
                }

                if (viewsToDelete.Count > 0)
                {
                    foreach (View curView in viewsToDelete)
                        doc.Delete(curView.Id);
                }

                tx.Commit();
            }

            Logger.Info("Add-in ended");

            try
            {
                FailedMethod();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured!");
            }

            return Result.Succeeded;
        }

        private void FailedMethod()
        {
            throw new NotImplementedException();
        }

        internal void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = @"C:\temp\Log.txt" };

            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;
        }
    }
}
