using System.Reflection;
using Autodesk.Revit.UI;
using SGTools.Commands;

namespace SGTools
{
    // Runs once when Revit starts, and builds the SG Tools 2.0 tab.
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // 1. Make the tab and a Sheets panel on it.
            application.CreateRibbonTab("SG Tools 2.0");
            RibbonPanel panel = application.CreateRibbonPanel("SG Tools 2.0", "Sheets");

            // 2. Add a button that runs HelloCommand from this dll.
            // 2. Add a button that runs ViewNumberTitle tool from this dll.
            string dllPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData(
                "ViewNumberTitle", "View Number\n+ Title", dllPath,
                typeof(ViewNumberTitleCommand).FullName);
            panel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}