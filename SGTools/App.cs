using System;
using System.Reflection;
using Autodesk.Revit.UI;
using SGTools.Commands;

namespace SGTools
{
    // The application: Revit runs OnStartup once, when Revit starts.
    // This is where an add-in builds its ribbon tab, panels and buttons.
    public class App : IExternalApplication
    {
        private const string TabName = "SG Tools 2.0";
        private const string HelpPage = "https://github.com/sonusgeorge/sg-csharp-tools";

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab(TabName);

            // One line per panel. A panel is a named group of buttons.
            RibbonPanel sheetsPanel = application.CreateRibbonPanel(TabName, "Sheets");

            // One AddButton call per button, on the panel it belongs to.
            AddButton(
                sheetsPanel,
                typeof(ViewNumberTitleCommand),
                "View Number\n+ Title",
                "view-number-title",
                "Renumber and retitle the views on a sheet.",
                "Changes a view's Detail Number and its Title on Sheet. If the number " +
                "already belongs to another view on the sheet, the two views swap numbers.\n\n" +
                "1. Open a sheet and click this button.\n" +
                "2. Click a view on the sheet.\n" +
                "3. Type the new number, the new title, or both. Press Enter.\n" +
                "4. Click the next view, or press Esc to finish.\n\n" +
                "Each change is one undo step. A legend has no number, so only its " +
                "title can be changed. Press F1 for the full guide.",
                HelpPage + "#view-number--title");

            return Result.Succeeded;
        }

        // Revit runs this once, when it closes. We have nothing to tidy up.
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        // Adds one button to a panel.
        //   command      the class Revit runs when the button is clicked
        //   text         the words under the button
        //   iconName     the picture's file name in Resources, without ".png"
        //   tooltip      the short line shown on hover
        //   description  the longer text shown when you keep hovering
        //   helpUrl      the page F1 opens
        private static void AddButton(RibbonPanel panel, Type command, string text,
                                      string iconName, string tooltip, string description,
                                      string helpUrl)
        {
            // A button has to know which dll holds its command, and which
            // class in that dll to run. FullName is "namespace.class".
            string dllPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData button = new PushButtonData(command.FullName, text, dllPath, command.FullName);

            button.LargeImage = Icons.Load(iconName);
            button.ToolTip = tooltip;
            button.LongDescription = description;
            button.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, helpUrl));

            panel.AddItem(button);
        }
    }
}
