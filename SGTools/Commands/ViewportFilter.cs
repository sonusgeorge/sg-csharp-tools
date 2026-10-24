using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace SGTools.Commands
{
    // Tells Revit which elements the user is allowed to click:
    // only views placed on a sheet (viewports).
    public class ViewportFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            return element is Viewport;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}