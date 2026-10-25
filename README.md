# SG Tools 2.0 (C#)

Revit add-ins written in C# against the Revit API. Each tool here is a rebuild of one I first made in pyRevit, in [sg-pyrevit-tools](https://github.com/sonusgeorge/sg-pyrevit-tools). Building the same tool twice is how I'm learning what pyRevit does for you and what a compiled add-in has to do for itself.

## Tools

### View Number + Title

**Ribbon:** SG Tools 2.0 tab, Sheets panel

**The problem.** Revit won't let two views on a sheet share a Detail Number. To put the views on a sheet in order you have to park one view on a throwaway number, renumber the next, and come back to the first.

**What the tool does.** Pick a view on the sheet and type its new number. If another view already has that number, the two swap, and the tool handles the temporary number itself. The same box also changes the view's Title on Sheet.

It behaves the same as the pyRevit version; the [recording there](https://github.com/sonusgeorge/sg-pyrevit-tools#view-number--title) shows it in use.

**How to use it**

1. Open a sheet and click **View Number + Title**.
2. Click a view on the sheet.
3. Type the new number, the new title, or both. A notice tells you before you apply if the number belongs to another view and the two will swap.
4. Press Enter to apply. The tool goes straight back to waiting for the next view.
5. Press Esc when you're done.

Each change is one undo step. A legend has no Detail Number, so for a legend only the title can be changed.

While it waits for a click, a teal banner at the top of the drawing area says what to do. The pyRevit version uses the same banner.

**Tested in:** Revit 2024 and Revit 2027, in full. Written to be compatible with Revit 2025 and 2026: it builds for both, but I haven't run it in them yet.

## The same tool, two ways

| | pyRevit | C# |
|---|---|---|
| How Revit runs it | pyRevit reads the script on every click | The code is compiled into a `.dll` that Revit loads at startup |
| Trying a change | Edit and click again | Close Revit, build, reopen |
| Ribbon tab and buttons | Folder names | Code in `App.cs` |
| Button picture | A file named `icon.png` | A picture packed into the `.dll` and loaded in code |
| Telling Revit it exists | Done by pyRevit | A hand-written `.addin` manifest |
| Transactions | `with revit.Transaction(...)` | `Start`, `Commit` and `RollBack` written out |
| The box | XAML file loaded by a Python class | The same XAML, compiled together with a C# class |
| Mistakes in names or types | Found when the line runs | Most are found when building |
| Several Revit versions | The same script runs in every version | One `.dll` per Revit version, each built from the same code |

## How the project is laid out

```
SGTools/
  SGTools.csproj      project settings: which Revit, its .NET version, the Revit API packages, the copy-to-Revit step
  SGTools.addin       the manifest Revit reads at startup
  App.cs              runs at startup; builds the tab, panels and buttons
  Icons.cs            loads button pictures out of the dll
  Commands/           one class per tool
  Dialogs/            windows: a .xaml layout plus its .xaml.cs logic
  Styles/             the shared SG Tools colours and styles
  Resources/          button pictures
```

## Build and install

You need the .NET 10 SDK. Revit itself isn't needed to build: the Revit API files come from NuGet (the `Nice3point.Revit.Api` packages), in the version that matches each Revit.

1. Clone this repository.
2. Close Revit.
3. In a terminal, go into the `SGTools` folder and build for your Revit version:

   | Revit | Command | .NET |
   |---|---|---|
   | 2027 | `dotnet build` | .NET 10 |
   | 2026 | `dotnet build -p:RevitVersion=2026` | .NET 8 |
   | 2025 | `dotnet build -p:RevitVersion=2025` | .NET 8 |
   | 2024 | `dotnet build -p:RevitVersion=2024` | .NET Framework 4.8 |

   The build goes to `bin\<version>`. If that Revit is installed, it is also copied into `%APPDATA%\Autodesk\Revit\Addins\<version>`; if not, the build says so with a warning.
4. Start Revit. When it warns about an unsigned add-in, choose **Always Load**. The **SG Tools 2.0** tab appears on the ribbon.

To build every version in one go (PowerShell):

```powershell
foreach ($v in 2024, 2025, 2026, 2027) { dotnet build -p:RevitVersion=$v }
```

To check that the code compiles while Revit is open, add `-p:SkipRevitCopy=true`, which skips the copy into Revit's folder. You can also open `SGTools/SGTools.csproj` in Visual Studio and build from there; it builds for Revit 2027.

## What building it taught

- A Revit add-in is two things: a `.dll` with the code, and a small `.addin` manifest that tells Revit where the `.dll` is and which class to run.
- An add-in registers either as a command, which is one action under External Tools, or as an application, which runs at startup and can build its own ribbon tab.
- C# has to be compiled before Revit can use it, and a running Revit keeps the `.dll` locked, so every change means closing Revit first.
- C# makes you state the type of everything. A general element has to be cast, as in `(Viewport)doc.GetElement(...)`, before it can be used as a viewport.
- A window is one class split over two files, the XAML layout and the C# logic, joined by the word `partial`.
- pyRevit quietly does a lot: it loads the Revit API, opens and closes transactions, builds the ribbon from folders and makes Revit the owner of every window. In C# each of those has to be written out.
- The project file can be written by hand in about thirty lines: the .NET version, the two Revit API files, WPF, and a step that copies the add-in into Revit's folder after every build.
- The compiler catches a missing or misspelled name before Revit ever runs the code, but not a name written inside text. A styles file saved under the wrong name built fine and only failed when the window opened.
- C#'s `??` only replaces `null`. Python's `or` also treats an empty string as missing, so the C# version needs `string.IsNullOrEmpty` to do the same job.
- Two different classes can share a name. .NET and Revit both have an `OperationCanceledException`, so catching Esc needs Revit's full name, or the tool would catch the wrong one and crash.
- Code can subscribe to Revit's own events. Listening for `ThemeChanged` lets the button swap to its dark-theme picture the moment the theme changes.
- A window shown with `Show()` doesn't wait, unlike `ShowDialog()`, so the banner stays up while Revit waits for a click. Closing it in a `finally` block means it disappears however the tool ends.
- The window only receives text, never Revit objects. The command does all the Revit work, so each file has one job.

### Building for several Revit versions

- A `.dll` fits one Revit and one .NET: Revit 2024 runs on .NET Framework 4.8, 2025 and 2026 on .NET 8, 2027 on .NET 10. So the project builds once per version, from the same code, choosing .NET from a single `RevitVersion` setting.
- You can build for a Revit you don't have. The API files come from NuGet in every version, so building needs no Revit at all. Running it is another matter, which is why 2025 and 2026 are "written to be compatible with", not "tested".
- Project files have their own small language: settings you name yourself, `$(Name)` to read them, and `Condition` to use a line only when a test is true.
- Don't trust a folder to tell you what's installed. pyRevit had created `Addins\2026` on a machine without Revit 2026, so the copy step checks for `Revit.exe` instead.
- Older .NET means older C#. .NET Framework projects use C# 7.3, which has no `is not`; the 2024 build failed until `x is not ViewSheet` became `!(x is ViewSheet)`, which works everywhere.
- `dotnet build` hides plain messages in its short output, so a note that must be seen has to be a warning.
- A build only recompiles when a source file changed, and copying keeps the file's original time. An unchanged time in Revit's folder doesn't mean the build failed.

## How these tools are made

I plan each tool and decide how it should behave, and an AI assistant (Claude) helps with the code: it explains each piece, and I type it in myself, one small piece at a time, building and testing in Revit as I go. I don't move on until I can explain what every part does. The first C# version of View Number + Title was written by the assistant; I then rebuilt it from a blank folder this way, starting with the project file and a Hello button. The steps are adapted from Erik Frits's PROCESS framework for pyRevit tools.
