using System.Reflection;
using Autodesk.Revit.UI;
using SGTools.Commands;
using Autodesk.Revit.UI.Events;

namespace SGTools
{
    // Runs once when Revit starts, and builds the SG Tools 2.0 tab.
    public class App : IExternalApplication
    {
        // Kept so its picture can be swapped when Revit's theme changes.
        private PushButton viewNumberButton;        
        public Result OnStartup(UIControlledApplication application)
        {
            // 1. Make the tab and a Sheets panel on it.
            application.CreateRibbonTab("SG Tools 2.0");
            RibbonPanel panel = application.CreateRibbonPanel("SG Tools 2.0", "Sheets");

            // 2. Add a button that runs ViewNumberTitle tool from this dll.
            string dllPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData(
                "ViewNumberTitle", "View Number\n+ Title", dllPath,
                typeof(ViewNumberTitleCommand).FullName);
            // The short line shown on hover, and the longer text shown
            // when you keep hovering.
            buttonData.ToolTip = "Renumber and retitle the views on a sheet.";
            buttonData.LongDescription =
                "Changes a view's Detail Number and its Title on Sheet. If the number " +
                "already belongs to another view on the sheet, the two views swap numbers.\n\n" +
                "1. Open a sheet and click this button.\n" +
                "2. Click a view on the sheet.\n" +
                "3. Type the new number, the new title, or both. Press Enter.\n" +
                "4. Click the next view, or press Esc to finish.\n\n" +
                "Each change is one undo step. A legend has no number, so only its " +
                "title can be changed.\n\n" +
                "Version 1.1 (October 2026). Tested in Revit 2024 and 2027. Press F1 for the full guide.";

            // F1 while hovering opens the tool's section of the README.
            buttonData.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url,
                "https://github.com/sonusgeorge/sg-csharp-tools#view-number--title"));
            buttonData.LargeImage = Icons.Load("view-number-title");
            viewNumberButton = (PushButton)panel.AddItem(buttonData);

            // Revit's light/dark theme can change while it runs.
            // When it does, run OnThemeChanged.
            application.ThemeChanged += OnThemeChanged;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ThemeChanged -= OnThemeChanged;            
            return Result.Succeeded;
        }
        // Load the picture again, which picks the one for the new theme.
        private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            viewNumberButton.LargeImage = Icons.Load("view-number-title");
        }
    }
}