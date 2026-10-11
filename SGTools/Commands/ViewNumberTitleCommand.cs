using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SGTools.Dialogs;

namespace SGTools.Commands
{
    // Change the Detail Number and Title on Sheet of views on a sheet.
    // The C# version of the pyRevit tool, following the same eight steps.
    [Transaction(TransactionMode.Manual)]
    public class ViewNumberTitleCommand : IExternalCommand
    {
        private const string ToolName = "View Number + Title";

        // Revit refuses two views with the same number on one sheet, even
        // for a moment, so a swap parks one view on this number in between.
        private const string TempNumber = "SG_TEMP_SWAP";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // uidoc is the model as shown on screen, doc is the model itself.
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // 1. Check that the open view is a sheet. If it isn't, say so and stop.
            if (doc.ActiveView is not ViewSheet)
            {
                TaskDialog.Show(ToolName,
                    "Open a sheet first, then run this tool.\n\n" +
                    "If you are working inside a view on a sheet, deactivate it first " +
                    "(right-click, Deactivate View).");
                return Result.Cancelled;
            }

            // 2. Show a banner at the top of the drawing area. "finally"
            //    guarantees it is removed however the tool ends.
            PickBanner banner = new PickBanner("Pick a view to renumber", commandData.Application);
            banner.Show();
            try
            {
                while (true)
                {
                    // 3. Wait for a click on a view. Esc ends the tool.
                    Reference reference;
                    try
                    {
                        reference = uidoc.Selection.PickObject(
                            ObjectType.Element,
                            new ViewportFilter(),
                            "Pick a view to renumber. Esc to finish.");
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        break;
                    }
                    Viewport picked = (Viewport)doc.GetElement(reference);
                    View view = (View)doc.GetElement(picked.ViewId);

                    // 4. Read the picked view's current number and title.
                    string oldNumber = GetNumber(picked);
                    string oldTitle = GetTitleOnSheet(view);

                    // 5. Note which number each of the other views on this sheet has.
                    //    Views without a number (legends) are left out.
                    ViewSheet sheet = (ViewSheet)doc.GetElement(picked.SheetId);
                    Dictionary<string, Viewport> others = new Dictionary<string, Viewport>();
                    Dictionary<string, string> numbersInUse = new Dictionary<string, string>();
                    foreach (ElementId viewportId in sheet.GetAllViewports())
                    {
                        Viewport viewport = (Viewport)doc.GetElement(viewportId);
                        string number = GetNumber(viewport);
                        if (viewport.Id != picked.Id && number != "")
                        {
                            others[number] = viewport;
                            numbersInUse[number] = GetShownTitle(doc, viewport);
                        }
                    }

                    // 6. Show the box and wait for Apply or Cancel.
                    ViewTitleWindow window = new ViewTitleWindow(
                        view.Name, DescribeViewType(view), oldNumber, oldTitle, numbersInUse);
                    // Make Revit the box's owner, so it opens centred on Revit
                    // and stays in front of it.
                    new WindowInteropHelper(window).Owner = commandData.Application.MainWindowHandle;
                    window.ShowDialog();

                    // Cancelled or nothing changed: change nothing, pick again.
                    if (!window.Applied)
                    {
                        continue;
                    }
                    string newNumber = window.NewNumber;
                    string newTitle = window.NewTitle;
                    if (newNumber == oldNumber && newTitle == oldTitle)
                    {
                        continue;
                    }

                    // 7. Make the changes as one undo step. If Revit refuses any
                    //    of them, undo them all and say so.
                    using (Transaction transaction = new Transaction(doc, ToolName))
                    {
                        transaction.Start();
                        try
                        {
                            if (newNumber != oldNumber)
                            {
                                if (others.TryGetValue(newNumber, out Viewport other))
                                {
                                    SetNumber(other, FreeTempNumber(others));
                                    SetNumber(picked, newNumber);
                                    SetNumber(other, oldNumber);
                                }
                                else
                                {
                                    SetNumber(picked, newNumber);
                                }
                            }
                            if (newTitle != oldTitle)
                            {
                                view.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(newTitle);
                            }
                            transaction.Commit();
                        }
                        catch (Exception error)
                        {
                            transaction.RollBack();
                            TaskDialog.Show(ToolName,
                                "Revit refused the change, so nothing was changed.\n\n" + error.Message);
                        }
                    }

                    // 8. Back to step 3.
                }
            }
            finally
            {
                banner.Close();
            }

            return Result.Succeeded;
        }

        // The view's Detail Number. Empty text for a view that has none,
        // such as a legend.
        private static string GetNumber(Viewport viewport)
        {
            return viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).AsString() ?? "";
        }

        // Set() returns false when Revit refuses the value. Throwing an
        // error here sends us to the "catch" block, which undoes everything.
        private static void SetNumber(Viewport viewport, string number)
        {
            if (!viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).Set(number))
            {
                throw new InvalidOperationException("Revit refused the number '" + number + "'.");
            }
        }

        // A temporary number that no view on the sheet is using.
        private static string FreeTempNumber(Dictionary<string, Viewport> numbersInUse)
        {
            string number = TempNumber;
            while (numbersInUse.ContainsKey(number))
            {
                number += "_";
            }
            return number;
        }

        // What is typed in Title on Sheet. Empty text if nothing is.
        private static string GetTitleOnSheet(View view)
        {
            return view.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).AsString() ?? "";
        }

        // The title you see on the sheet: Title on Sheet, or the view name.
        private static string GetShownTitle(Document doc, Viewport viewport)
        {
            View view = (View)doc.GetElement(viewport.ViewId);
            string title = GetTitleOnSheet(view);
            return title != "" ? title : view.Name;
        }

        // Turn Revit's "FloorPlan" into "floor plan" for the box.
        private static string DescribeViewType(View view)
        {
            switch (view.ViewType)
            {
                case ViewType.ThreeD:
                    return "3D view";
                case ViewType.EngineeringPlan:
                    return "structural plan";
                default:
                    return Regex.Replace(view.ViewType.ToString(), "([a-z])([A-Z])", "$1 $2").ToLower();
            }
        }
    }
}
