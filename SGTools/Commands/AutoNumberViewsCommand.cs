using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace SGTools.Commands
{
    // Number the views on a sheet 1, 2, 3... by where they sit.
    [Transaction(TransactionMode.Manual)]
    public class AutoNumberViewsCommand : IExternalCommand
    {
        private const string ToolName = "Auto Number Views";

        public Result Execute(ExternalCommandData commandData,
                              ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // 1. Check that the open view is a sheet. If it isn't, say so and stop.
            ViewSheet sheet = doc.ActiveView as ViewSheet;
            if (sheet == null)
            {
                TaskDialog.Show(ToolName,
                    "Open a sheet first, then run this tool.\n\n" +
                    "If you are working inside a view on a sheet, deactivate it first " +
                    "(right-click, Deactivate View).");
                return Result.Cancelled;
            }

            // 2. Get the sheet's views, leaving out those with no number (legends).
            //    If none are left, say so and stop.
            List<Viewport> viewports = new List<Viewport>();
            foreach (ElementId viewportId in sheet.GetAllViewports())
            {
                Viewport viewport = (Viewport)doc.GetElement(viewportId);
                string number = viewport
                    .get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER)
                    .AsString() ?? "";
                if (number != "")
                {
                    viewports.Add(viewport);
                }
            }
            if (viewports.Count == 0)
            {
                TaskDialog.Show(ToolName, "This sheet has no numbered views to renumber.");
                return Result.Cancelled;
            }

            // Round 1 only: say what was found. Steps 3 to 7 replace this.
            TaskDialog.Show(ToolName,
                $"Found {viewports.Count} numbered views on sheet {sheet.SheetNumber}.");
            return Result.Succeeded;
        }
    }
}