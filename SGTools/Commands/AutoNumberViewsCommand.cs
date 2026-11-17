using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

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
            // 3. Ask for the reading order. Cancel, Esc or the × stops the tool.
            TaskDialog ask = new TaskDialog(ToolName);
            ask.MainInstruction = "Number the views on this sheet:";
            ask.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Columns first",
                "Each column top to bottom, columns left to right.");
            ask.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Rows first",
                "Each row left to right, rows top to bottom.");
            ask.CommonButtons = TaskDialogCommonButtons.Cancel;
            TaskDialogResult answer = ask.Show();

            string choice;
            if (answer == TaskDialogResult.CommandLink1)
            {
                choice = "Columns first";
            }
            else if (answer == TaskDialogResult.CommandLink2)
            {
                choice = "Rows first";
            }
            else
            {
                return Result.Cancelled;
            }            
            // 4 and 5. Group the views into columns or rows, then put them
            //    in reading order.
            List<Viewport> ordered = new List<Viewport>();
            if (choice == "Columns first")
            {
                // Columns left to right, each column top to bottom.
                foreach (List<Viewport> column in Group(viewports, "x"))
                {
                    ordered.AddRange(column.OrderByDescending(v => Centre(v, "y")));
                }
            }
            else
            {
                // Rows top to bottom, each row left to right.
                List<List<Viewport>> rows = Group(viewports, "y");
                rows.Reverse();
                foreach (List<Viewport> row in rows)
                {
                    ordered.AddRange(row.OrderBy(v => Centre(v, "x")));
                }
            }

            // Round 2 only: show the new order without changing anything.
            string text = choice + ":\n";
            for (int i = 0; i < ordered.Count; i++)
            {
                Viewport viewport = ordered[i];
                string name = doc.GetElement(viewport.ViewId).Name;
                string now = viewport
                    .get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER)
                    .AsString();
                text += $"\n{i + 1}.  {name}  (now {now})";
            }
            TaskDialog.Show(ToolName, text);
            return Result.Succeeded;
        }
        // Where the middle of the view's box is on the sheet, across ("x")
        // or up ("y").
        private static double Centre(Viewport viewport, string axis)
        {
            XYZ point = viewport.GetBoxCenter();
            return axis == "x" ? point.X : point.Y;
        }

        // Where the view's box starts and ends on the sheet, across ("x")
        // or up ("y").
        private static (double low, double high) Span(Viewport viewport, string axis)
        {
            Outline box = viewport.GetBoxOutline();
            if (axis == "x")
            {
                return (box.MinimumPoint.X, box.MaximumPoint.X);
            }
            return (box.MinimumPoint.Y, box.MaximumPoint.Y);
        }  
        // Split the views into columns (axis "x") or rows (axis "y").
        // Going from left to right (or bottom to top), a view joins the last
        // group if its centre falls within the box of that group's first view;
        // otherwise it starts a new group.
        private static List<List<Viewport>> Group(List<Viewport> viewports, string axis)
        {
            List<List<Viewport>> groups = new List<List<Viewport>>();
            foreach (Viewport viewport in viewports.OrderBy(v => Centre(v, axis)))
            {
                if (groups.Count > 0)
                {
                    List<Viewport> last = groups[groups.Count - 1];
                    (double low, double high) = Span(last[0], axis);
                    double centre = Centre(viewport, axis);
                    if (low <= centre && centre <= high)
                    {
                        last.Add(viewport);
                        continue;
                    }
                }
                groups.Add(new List<Viewport> { viewport });
            }
            return groups;
        }      
    }
}