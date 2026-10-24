using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SGTools.Commands
{
    // Runs when the Hello button is clicked.
    [Transaction(TransactionMode.Manual)]
    public class HelloCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
                              ref string message, ElementSet elements)
        {
            TaskDialog.Show("SG Tools 2.0", "Hello again from C#");
            return Result.Succeeded;
        }
    }
}