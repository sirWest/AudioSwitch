; Build the payload first: powershell -File ..\tools\Publish.ps1
; Inno Setup 6.3 or later. All source paths are relative to this script.
#define ApplicationName "AudioSwitch"
#define PublishDirectory AddBackslash(SourcePath) + "..\artifacts\publish\win-x64"
#if !FileExists(PublishDirectory + "\AudioSwitch.dll")
  #error Run tools\Publish.ps1 before compiling the installer.
#endif
#define ApplicationVersion GetStringFileInfo(PublishDirectory + "\AudioSwitch.dll", "ProductVersion")
#define ApplicationFileVersion GetVersionNumbersString(PublishDirectory + "\AudioSwitch.dll")

[Setup]
AppId={#ApplicationName}
AppName={#ApplicationName}
AppVerName={#ApplicationName} {#ApplicationVersion}
AppVersion={#ApplicationVersion}
VersionInfoVersion={#ApplicationFileVersion}
AppPublisher=Tanel Teede
AppMutex=Local\AudioSwitch.Tray
DefaultDirName={localappdata}\{#ApplicationName}
DefaultGroupName={#ApplicationName}
UninstallDisplayIcon={app}\AudioSwitch.exe
OutputDir=..\artifacts\installer
OutputBaseFilename=AudioSwitchSetup-{#ApplicationVersion}-win-x64
SetupIconFile=AudioSwitchIcon.ico
WizardImageFile=SideImage.bmp
WizardSmallImageFile=SmallImage.bmp
WizardStyle=modern
DisableWelcomePage=no
DisableReadyPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.14393
SolidCompression=yes
Compression=lzma2/ultra64
CloseApplications=yes
CloseApplicationsFilter=AudioSwitch.exe,audioswitch-cli.exe
RestartApplications=no

[Files]
Source: "{#PublishDirectory}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: "startupicon"; Description: "Start AudioSwitch when I log in to Windows"; GroupDescription: "Additional options:"

[Icons]
Name: "{group}\AudioSwitch"; Filename: "{app}\AudioSwitch.exe"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"

[Registry]
; Match the application's General settings instead of creating a second startup mechanism.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "AudioSwitch"; ValueData: """{app}\AudioSwitch.exe"" --startup"; Flags: uninsdeletevalue; Tasks: startupicon
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "AudioSwitch"; Flags: deletevalue; Tasks: not startupicon

[InstallDelete]
; Replace the legacy startup shortcut with the shared Run entry.
Type: files; Name: "{userstartup}\AudioSwitch.lnk"

[Run]
Filename: "{app}\AudioSwitch.exe"; Description: "Run AudioSwitch"; Flags: runasoriginaluser postinstall nowait skipifsilent
Filename: "https://www.paypal.com/donate/?hosted_button_id=2KV5RQBSB8Z9A"; Description: "Go and donate a small amount :)"; Flags: shellexec runasoriginaluser postinstall skipifsilent

[Code]
function HasDesktopRuntimeInRegistry(RootKey: Integer): Boolean;
var
  Versions: TArrayOfString;
  I: Integer;
  Installed: Cardinal;
  Key: String;
begin
  Result := False;
  Key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  if RegGetValueNames(RootKey, Key, Versions) then
    for I := 0 to GetArrayLength(Versions) - 1 do
      if (Copy(Versions[I], 1, 5) = '10.0.') and (Pos('-', Versions[I]) = 0) then
        if RegQueryDWordValue(RootKey, Key, Versions[I], Installed) and (Installed = 1) then
        begin
          Result := True;
          Exit;
        end;
end;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  { Modern .NET is registered in the 32-bit view, even for the x64 runtime.
    Check both views; .NET Framework, ASP.NET-only and x86 installs do not qualify. }
  Result := HasDesktopRuntimeInRegistry(HKLM32) or HasDesktopRuntimeInRegistry(HKLM64);
  if not Result then
  begin
    if WizardSilent then
      Log('AudioSwitch requires Microsoft .NET 10 Desktop Runtime (x64).')
    else if MsgBox('AudioSwitch requires Microsoft .NET 10 Desktop Runtime (x64).'#13#10#13#10 +
      'Install the Desktop Runtime, then run setup again. Open the download page now?',
      mbConfirmation, MB_YESNO) = IDYES then
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/10.0',
        '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
  end;
end;
