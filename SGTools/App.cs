using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.Revit.UI;
using SGTools.Commands;
using Autodesk.Revit.UI.Events;

namespace SGTools
{
    // Runs once when Revit starts, and builds the SG Tools 2.0 tab.
    public class App : IExternalApplication
    {
        // Every button and the name of its picture, kept so the pictures
        // can be swapped when Revit's theme changes.
        private Dictionary<PushButton, string> buttonIcons = new Dictionary<PushButton, string>();      
        public Result OnStartup(UIControlledApplication application)
        {
            // 1. Make the tab and a Sheets panel on it.
            application.CreateRibbonTab("SG Tools 2.0");
            RibbonPanel panel = application.CreateRibbonPanel("SG Tools 2.0", "Sheets");

            // 2. Add a button for each tool.
            AddButton(panel, "ViewNumberTitle", "View Number\n+ Title",
                typeof(ViewNumberTitleCommand), "view-number-title",
                "Renumber and retitle the views on a sheet.",
                "Changes a view's Detail Number and its Title on Sheet. If the number " +
                "already belongs to another view on the sheet, the two views swap numbers.\n\n" +
                "1. Open a sheet and click this button.\n" +
                "2. Click a view on the sheet.\n" +
                "3. Type the new number, the new title, or both. Press Enter.\n" +
                "4. Click the next view, or press Esc to finish.\n\n" +
                "Each change is one undo step. A legend has no number, so only its " +
                "title can be changed.\n\n" +
                "Version 1.1 (October 2026). Tested in Revit 2024 and 2027. Press F1 for the full guide.",
                "https://github.com/sonusgeorge/sg-csharp-tools#view-number--title");

            AddButton(panel, "AutoNumberViews", "Auto Number\nViews",
                typeof(AutoNumberViewsCommand), "auto-number-views",
                "Number the views on a sheet by where they sit.",
                "Work in progress.",
                "https://github.com/sonusgeorge/sg-csharp-tools#auto-number-views");

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
        // Make one button on the panel that runs the given command from this dll.
        private void AddButton(RibbonPanel panel, string name, string text,
                               Type command, string icon,
                               string toolTip, string longDescription, string helpUrl)
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData(name, text, dllPath, command.FullName);
            // The short line shown on hover, and the longer text shown
            // when you keep hovering.
            buttonData.ToolTip = toolTip;
            buttonData.LongDescription = longDescription;
            // F1 while hovering opens the tool's section of the README.
            buttonData.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, helpUrl));
            buttonData.LargeImage = Icons.Load(icon);

            PushButton button = (PushButton)panel.AddItem(buttonData);
            buttonIcons[button] = icon;
        }        
        // Load every button's picture again, which picks the one for the new theme.
        private void OnThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            foreach (KeyValuePair<PushButton, string> pair in buttonIcons)
            {
                pair.Key.LargeImage = Icons.Load(pair.Value);
            }
        }
    }
}