using Autodesk.Revit.DB;
using System;

namespace SGTools.Commands
{
    // Reading and safely setting the Detail Numbers of views on a sheet.
    // Shared by the SG Tools buttons that renumber views.
    public static class Viewports
    {
        // Revit refuses two views with the same number on one sheet, even for
        // a moment, so a view is parked on a temporary number while others move.
        // Each temporary number starts with this, followed by something unique.
        public const string TempPrefix = "SG_TEMP_";

        // A view's Detail Number, or "" if it has none (a legend).
        public static string GetNumber(Viewport viewport)
        {
            return viewport
                .get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER)
                .AsString() ?? "";
        }

        // Set a view's Detail Number. If Revit refuses, throw an error, so
        // the transaction undoes everything done so far.
        public static void SetNumber(Viewport viewport, string number)
        {
            Parameter param = viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER);
            if (!param.Set(number))
            {
                throw new InvalidOperationException($"Revit refused the number '{number}'.");
            }
        }
    }
}