; SG Tools 2.0 installer, built with Inno Setup.
; Lines starting with ; are comments.

; Read the version from the Release dll, so it is only written in the
; .csproj. Paths are relative to this file.
#define DllFor2027 "..\SGTools\bin\Release\2027\SGTools.dll"
#define AppVersion GetStringFileInfo(DllFor2027, "ProductVersion")

[Setup]
; Identifies SG Tools to Windows. Never change it: a newer installer
; with the same ID replaces the old install instead of adding a second.
AppId=6c1e195e-a57c-4d5b-ad46-8d2ed8818686
AppName=SG Tools 2.0
AppVersion={#AppVersion}
AppPublisher=Sonu George
AppPublisherURL=https://github.com/sonusgeorge/sg-csharp-tools

; Install for the current user only, so no administrator prompt.
PrivilegesRequired=lowest

; Where the uninstaller lives. The add-in itself goes to Revit's folders.
DefaultDirName={autopf}\SG Tools 2.0
DisableDirPage=yes
DisableProgramGroupPage=yes

; The finished installer: installer\Output\SGToolsSetup-1.1.0.exe
OutputDir=Output
OutputBaseFilename=SGToolsSetup-{#AppVersion}
WizardStyle=modern

[Files]
; For each Revit version: the dll goes in Addins\<version>\SGTools, the
; manifest in Addins\<version>. "Check" skips a Revit that isn't installed.
; "ignoreversion" always replaces an older copy that is already there.
Source: "..\SGTools\bin\Release\2024\SGTools.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2024\SGTools"; Flags: ignoreversion; Check: RevitInstalled('2024')
Source: "..\SGTools\SGTools.addin";                DestDir: "{userappdata}\Autodesk\Revit\Addins\2024";         Flags: ignoreversion; Check: RevitInstalled('2024')
Source: "..\SGTools\bin\Release\2025\SGTools.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2025\SGTools"; Flags: ignoreversion; Check: RevitInstalled('2025')
Source: "..\SGTools\SGTools.addin";                DestDir: "{userappdata}\Autodesk\Revit\Addins\2025";         Flags: ignoreversion; Check: RevitInstalled('2025')
Source: "..\SGTools\bin\Release\2026\SGTools.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2026\SGTools"; Flags: ignoreversion; Check: RevitInstalled('2026')
Source: "..\SGTools\SGTools.addin";                DestDir: "{userappdata}\Autodesk\Revit\Addins\2026";         Flags: ignoreversion; Check: RevitInstalled('2026')
Source: "..\SGTools\bin\Release\2027\SGTools.dll"; DestDir: "{userappdata}\Autodesk\Revit\Addins\2027\SGTools"; Flags: ignoreversion; Check: RevitInstalled('2027')
Source: "..\SGTools\SGTools.addin";                DestDir: "{userappdata}\Autodesk\Revit\Addins\2027";         Flags: ignoreversion; Check: RevitInstalled('2027')

[Code]
// True if this Revit version is installed. The same test as the copy
// step in SGTools.csproj: does Revit.exe exist?
function RevitInstalled(Version: String): Boolean;
begin
  Result := FileExists(ExpandConstant('{commonpf64}\Autodesk\Revit ' + Version + '\Revit.exe'));
end;