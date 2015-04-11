#define ApplicationName 'AudioSwitch'
#define ApplicationVersion GetFileVersion('..\bin\Release\AudioSwitch.exe')

[Setup]
AppId=ApplicationName
AppName={#ApplicationName} v2.0
AppVerName={#ApplicationName}
AppVersion={#ApplicationVersion}
VersionInfoVersion={#ApplicationVersion}
DefaultDirName={localappdata}\{#ApplicationName}
UninstallDisplayIcon={app}\AudioSwitch.exe
DefaultGroupName=AudioSwitch
OutputDir=.
OutputBaseFilename=AudioSwitchSetup
WizardImageFile=WizModernImage-IS.bmp
WizardSmallImageFile=WizModernSmallImage-IS.bmp
PrivilegesRequired=lowest
disablereadypage=yes
solidcompression=yes
compression=lzma2/ultra64
MinVersion=6.1

[Files]
Source: "..\bin\Release\AudioSwitch.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\Settings.xml"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall
Source: "..\bin\Release\Skins\*"; DestDir: "{app}\Skins"; Flags: recursesubdirs

[Tasks]
Name: "startupicon"; Description: "Start AudioSwitch when I log in to Windows"; GroupDescription: "Additional options:"

[Icons]
Name: "{group}\AudioSwitch"; Filename: "{app}\AudioSwitch.exe"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"
Name: "{userstartup}\AudioSwitch"; Filename: "{app}\AudioSwitch.exe"; Tasks: startupicon

[Run]
Filename: "{app}\AudioSwitch.exe"; Description: "Run AudioSwitch"; Flags: runasoriginaluser postinstall nowait skipifsilent

[Code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, serviceCount: cardinal;
    success: boolean;
begin
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;
    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;
    // .NET 4.0 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;
    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4\Client', 0) then begin
        MsgBox('AudioSwitch requires .NET Framework 4.0 Client Profile to be installed on your system.'#13#13
            'Please use Windows Update to install it and then run the setup again.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;