using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Windows.Interop;
using SGTools.Dialogs;
using System;
using System.Collections.Generic;

namespace SGTools.Commands
{
    // Change the Detail Number and Title on Sheet of views on a sheet.
    [Transaction(TransactionMode.Manual)]
    public class ViewNumberTitleCommand : IExternalCommand
    {
        private const string ToolName = "View Number + Title";
        // Revit refuses two views with the same number on one sheet, even
        // for a moment, so a swap parks one view on this number in between.
        private const string TempNumber = "SG_TEMP_SWAP";

        public Result Execute(ExternalCommandData commandData,
                              ref string message, ElementSet elements)
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
            // 2 to 7 run while a banner at the top of Revit says what to do.
            // "finally" closes it however the tool ends.
            PickBanner banner = new PickBanner("Pick a view to renumber", commandData.Application);
            banner.Show();
            try
            {
                while (true)
                {
                    // 2. Wait for a click on a view on the sheet. Esc stops the tool.
                    Reference reference;
                    try
                    {
                        reference = uidoc.Selection.PickObject(
                            ObjectType.Element,
                            new ViewportFilter(),
                            "Pick a view. Esc to finish.");
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        break;
                    }
                    Viewport viewport = (Viewport)doc.GetElement(reference);

                    // 3. Read the view's Detail Number and Title on Sheet.
                    Parameter numberParam = viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
                    Parameter titleParam = viewport.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION);
                    string oldNumber = numberParam.AsString() ?? "";
                    string oldTitle = titleParam.AsString() ?? "";

                    // 4. Note which number each of the other views on this sheet has.
                    //    Views without a number (legends) are left out.
                    ViewSheet sheet = (ViewSheet)doc.GetElement(viewport.SheetId);
                    Dictionary<string, Viewport> others = new Dictionary<string, Viewport>();
                    Dictionary<string, string> numbersInUse = new Dictionary<string, string>();
                    foreach (ElementId otherId in sheet.GetAllViewports())
                    {
                        Viewport otherViewport = (Viewport)doc.GetElement(otherId);
                        string number = otherViewport
                            .get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER)
                            .AsString() ?? "";
                        if (otherViewport.Id != viewport.Id && number != "")
                        {
                            others[number] = otherViewport;
                            numbersInUse[number] = ShownTitle(doc, otherViewport);
                        }
                    }

                    // 5. Show the box with the current values and wait for
                    //          Apply or Cancel. Cancel goes back to step 2.
                    View view = (View)doc.GetElement(viewport.ViewId);
                    ViewTitleWindow window = new ViewTitleWindow(view.Name, oldNumber, oldTitle, numbersInUse);
                    // Make Revit the box's owner, so it opens centred over Revit.
                    new WindowInteropHelper(window).Owner = commandData.Application.MainWindowHandle;
                    if (window.ShowDialog() != true)
                    {
                        continue;
                    }
                    string newNumber = window.NewNumber;
                    string newTitle = window.NewTitle;

                    // 6. If nothing changed, go back to step 2.
                    if (newNumber == oldNumber && newTitle == oldTitle)
                    {
                        continue;
                    }

                    // 7. Make the changes as one undo step. If Revit refuses any of
                    //    them, the transaction undoes them all and we say so.
                    using (Transaction transaction = new Transaction(doc, ToolName))
                    {
                        transaction.Start();
                        try
                        {
                            if (newNumber != oldNumber)
                            {
                                if (others.TryGetValue(newNumber, out Viewport swapWith))
                                {
                                    SetNumber(swapWith, TempNumber);
                                    SetNumber(viewport, newNumber);
                                    SetNumber(swapWith, oldNumber);
                                }
                                else
                                {
                                    SetNumber(viewport, newNumber);
                                }
                            }
                            if (newTitle != oldTitle)
                            {
                                titleParam.Set(newTitle);
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

                }
            }
            finally
            {
                banner.Close();
            }               

            return Result.Succeeded;
        }
        // Set a view's Detail Number. If Revit refuses, throw an error, so
        // the transaction undoes everything done so far.
        private static void SetNumber(Viewport viewport, string number)
        {
            Parameter param = viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
            if (!param.Set(number))
            {
                throw new InvalidOperationException($"Revit refused the number '{number}'.");
            }
        }
        // The title you see on the sheet: Title on Sheet, or the view's name
        // when Title on Sheet is empty.
        private static string ShownTitle(Document doc, Viewport viewport)
        {
            string title = viewport.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).AsString();
            if (string.IsNullOrEmpty(title))
            {
                return doc.GetElement(viewport.ViewId).Name;
            }
            return title;
        }
    }
}