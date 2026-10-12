# AudioSwitch Technical Reference

See the [main README](../README.md) for an introduction and testing information.

AudioSwitch is a Windows tray application and command-line tool for switching
audio devices, controlling volume, and configuring global shortcuts. It uses
.NET 10 and WPF, with no third-party runtime packages.

This reference describes the latest available release. See the
[releases page](https://github.com/sirWest/AudioSwitch/releases/latest) for downloads
and release notes.

## Requirements

- Windows 11, or a Windows 10 edition supported by .NET 10.
- The .NET 10 SDK to build from source.
- The .NET 10 Desktop Runtime to run the tray app. The CLI needs only the .NET runtime.
- Inno Setup 6.3 or later to build the installer.

The installer packages x64 executables and checks for the x64 Desktop Runtime,
including on Windows devices that support x64 emulation. See Microsoft's
[Windows installation guide](https://learn.microsoft.com/dotnet/core/install/windows)
for runtime downloads and supported Windows editions.

## Build and Run

Run these commands from the repository root:

```powershell
dotnet build AudioSwitch.sln
dotnet run --project AudioSwitch -- --settings
dotnet run --project AudioSwitch.Cli -- list --output --json
dotnet test AudioSwitch.sln
```

Open `AudioSwitch.sln` in Rider or Visual Studio to work with all four projects.
The legacy .NET Framework project has been removed.

| Project | Responsibility |
| --- | --- |
| `AudioSwitch` | WPF tray app, device flyout, settings, hotkeys, OSD and native shell integration |
| `AudioSwitch.Core` | Audio operations, Core Audio COM wrappers, settings, migration and window placement |
| `AudioSwitch.Cli` | Console executable, `audioswitch-cli.exe`, with text and JSON output |
| `AudioSwitch.Tests` | Regression tests and opt-in Windows audio/UI integration tests |

Application artwork lives in `AudioSwitch/Assets`; the shared application and
installer icon is `Setup/AudioSwitchIcon.ico`. These images are embedded in the
application assembly. The theme-aware default and 28 single-theme OSD skins live in `AudioSwitch/Skins`
and are copied into build and publish output. `Setup` holds the installer script
and its required artwork. README screenshots live in `docs/images` and are not packaged.

## Using AudioSwitch

Click the tray icon to open the device list. Ctrl+click opens the opposite device
group. Right-click opens the menu, or cycles devices when quick switching is
enabled; Shift+right-click always opens the menu. Scroll over the flyout to adjust
volume, and right-click its slider to toggle mute.

Settings include playback and recording preferences, custom names and hidden
devices, startup defaults, hotkeys, mouse-wheel modifiers, System/Dark/Light
appearance, tray icon colors and OSD skins. Appearance and OSD changes preview
before saving. Apply persists changes; Cancel restores the saved appearance.
The always-visible flyout and OSD preview can be dragged to a preferred position.

Tray arguments:

- `--settings`: open Settings.
- `--show`: open the device flyout.
- `--startup` or `/startup`: apply configured startup audio preferences.
- `--config <path>`: use an isolated profile without importing settings or modifying sign-in registration.
- `--exit-with-settings`: exit after the Settings window closes.
- `--show-tray-icon`: apply Windows 11 22H2+ tray visibility once, then run normally.
- `--exit`: exit without loading audio or settings. Combined with `--show-tray-icon`,
  briefly register the tray icon, apply visibility, and exit (0 on success, 1 on failure).
  An already-running instance is left running.

Use `audioswitch-cli --help` for the complete command list. Device IDs are
preferred for scripts; ambiguous device names are rejected. The CLI supports
legacy switch aliases, redirected stdout/stderr and exit codes: `0` for success,
`2` for invalid arguments and `3` for an operation or settings failure.

## Settings and Upgrades

### Device Hotkeys

The **Select audio devices** action stores independent playback and recording
endpoint IDs and mute policies. Either direction can be left unchanged while
muting its devices. Per-device exclusions apply to hotkey mute/unmute operations,
including toggle-mute shortcuts, but do not prevent device selection.

Targets are resolved on each press. Missing targets are skipped without blocking
the available member of a pair; a missing explicit target also skips its category's
mute-other-devices operation. No replacement device is selected automatically.
The communications checkbox initially uses the General setting and saves an
explicit choice for that hotkey. See the
[Device Hotkeys guide](DEVICE-HOTKEYS.md) for the user workflow.

### Device Icons

Playback and Recording settings each offer **Use device icon in tray**. This
uses the device icon reported by Windows, including icons customized in the
Windows Sound panel. **Browse...** imports an AudioSwitch-only icon without changing
the tray checkbox. **Reset icon** restores the Windows icon; uncheck the tray
option to return to the colored volume icon. A custom device-list icon can also
be kept while this option is off. Device-icon mode disables
the color sliders and volume-icon samples without discarding their saved values.
The tray tooltip always includes the displayed device name and volume percentage,
plus mute state. Muted device icons receive a light speaker-and-red-X overlay.

Imports support ICO, PNG, JPEG/JFIF, BMP, GIF, TIFF, and Windows HD Photo.
Animated and multipage images use their first frame; ICO files use their largest
frame. Images are fitted into transparent 256-pixel PNGs without changing aspect
ratio. Files above 32 MB or 40 million pixels are rejected.

Imports remain in memory until Apply or Save & Close. The first saved import
creates `DeviceIcons` beside the profile (normally
`%LOCALAPPDATA%/AudioSwitch/DeviceIcons`). SHA-256 filenames deduplicate identical
normalized images. Settings store only the filename. Cancel discards pending
imports; successful saves remove unreferenced managed PNGs while retaining images
referenced by backups, other readable profiles in that directory, and disconnected
devices. Unreadable JSON snapshots defer cleanup. Missing or damaged icon files
fall back to the Windows device icon. Windows device properties are not modified.

### Persistence

Settings are stored in `%LOCALAPPDATA%/AudioSwitch/settings.json`. On the first
normal tray launch, AudioSwitch imports
`%LOCALAPPDATA%/AudioSwitch/Settings.xml` from the legacy .NET Framework application.
The XML file is retained, and an existing JSON profile is never replaced. Launch the tray app
once to perform migration before using the CLI.

Writes use a cross-process mutex and atomic replacement. A `.bak` file retains
the preceding valid settings. Recovered corrupt files are preserved as
`.damaged-<timestamp>` on save. Unsupported schema versions remain untouched.
An editor cannot overwrite a newer CLI edit; reopen Settings to reload it.
The JSON schema version is independent of the application release version.

Errors are logged to `%LOCALAPPDATA%/AudioSwitch/errors.log`. Sign-in startup
uses the per-user `AudioSwitch` Run registry entry, shared with the installer.
Applying the startup preference removes the legacy startup shortcut.

## Publish and Package

In the IDE, publish the main `AudioSwitch` project using the **Installer** profile
(`AudioSwitch/Properties/PublishProfiles/Installer.pubxml`). It publishes a Release,
framework-dependent x64 build of the app and CLI, copies the skins and license,
and compiles `Setup/Setup.iss` in the same operation. Inno Setup must be installed
in its standard location or `ISCC.exe` must be available on `PATH`.

```powershell
# The same complete publish-and-installer operation from the command line:
dotnet publish AudioSwitch/AudioSwitch.csproj -c Release -p:PublishProfile=Installer

# Framework-dependent x64 app and CLI, skins and license:
./tools/Publish.ps1

# Publish and compile the installer:
./tools/Publish.ps1 -BuildInstaller
```

Publish output is `artifacts/publish/win-x64`. The installer is written to
`artifacts/installer/AudioSwitchSetup-<version>-win-x64.exe`. These generated files
are ignored by Git. Both the profile and publish script rebuild that publish
directory. The Installer profile uses this fixed output path; do not override it
with `-o`. A missing Inno Setup compiler or a failed packaging step fails publishing.

`Setup/Setup.iss` is the installer script. Setup installs per
user, shows the welcome screen, and checks the .NET 10 Desktop Runtime before
installation. It does not automatically download or install the runtime.
On Windows 11 22H2+, the selected tray-visibility task passes `--show-tray-icon`
to the application. If launch is declined or setup is silent, setup runs
`--show-tray-icon --exit` and waits for completion instead. The application waits
up to 10 seconds for Explorer's entry and updates only its own executable path.
This uses an undocumented Windows preference; failures are logged, and ordinary
launches do not override subsequent user choices. Windows 10 is unchanged.
The donation link on the finish page is optional and unchecked by default.

The only release version to edit is `<Version>` in `Directory.Build.props`.
The SDK derives the assembly, file, and informational versions from it. The build
stamps a generated copy of `AudioSwitch/app.manifest` with the assembly version;
the source manifest is a template and needs no release edits. The installer reads
its version from the published executable, so its filename follows automatically.

## Tests and Contributions

See [CONTRIBUTING.md](../CONTRIBUTING.md) for code organization, formatting, integration
tests and pull request guidance.

The default suite does not change system audio. Hardware switching, mixed-DPI
monitor placement, hot-plug, sleep/resume and full-screen games require a Windows
hardware pass.

### Native Volume OSD

With custom OSD disabled, playback volume and mute feedback request
the Windows media-key volume OSD; device changes use Windows notifications and
follow system notification settings. Recording volume and mute feedback require
the custom OSD, because the native volume display represents playback only.
The native OSD uses the private Immersive Shell flyout dispatcher on Windows 10/11;
availability depends on the Windows shell and session. If unavailable, volume and
mute changes still apply silently, without notification spam or simulated keys
that would change audio again. OSD windows are not guaranteed to appear over
exclusive full-screen games.

Custom OSD controls and previews are disabled while custom OSD is off.

## OSD Skin Themes

`Default` is a compact, icon-free light/dark skin. The previous default is
available as `Default (old)`. Skins without theme properties still use
`back.png`, `mute.png`, `meter.png` and optional `meter_effect.png` as before.

The 20 migrated legacy skins use version 2.0 with one shared color theme.
Horizontal sprite sheets, glass overlays, digital digits and the Windows 8
speaker effect are baked into vertical meter strips where needed. Author and
website credits remain in each skin's XML. `Grey volume` was already included.
All skin XML and PNG files are automatically included in build and publish output.

Optional image overrides in `skin.xml` follow the app's current appearance,
including System mode and unsaved appearance previews:

```xml
<Images DeviceBackground="device.png">
  <Light Background="back.png" Mute="mute.png" Meter="meter.png" />
  <Dark Background="back-dark.png" Mute="mute-dark.png" Meter="meter-dark.png"
        DeviceBackground="device-dark.png" />
</Images>
```

Each image resolves from the selected `Light`/`Dark` attributes, then the
shared `Images` attributes, then the legacy filename. `Effect` overrides
`meter_effect.png` when `VolBar Effect="true"`. `DeviceBackground` optionally
provides a clean surface for device text. Paths are relative to the skin folder
and must remain inside it. Keep matching theme/state images the same dimensions.
`DeviceText` accepts optional `ColorHexLight` and `ColorHexDark`, falling back to
`ColorHex`. Horizontal `VolBar` accepts optional `CornerRadius` for a rounded fill.

Device labels are laid out per skin. `DeviceText` supports `Alignment` (Left,
Center, Right), `VerticalAlignment` (Top, Center, Bottom), `Wrap` (default true),
`Shadow` (default false), `BackgroundHex` (optional ARGB color) and `Padding`
(default 0). Coordinates and maximum dimensions are in pixels; font size is in
points. The label uses the original artwork where possible, including the right
panel of DigitalControl. Compact skins can place the label beside their artwork:
the device notification expands to contain the configured label area, then returns
to the original size for volume and mute. These settings do not require separate
light/dark skin variants.

`OsdSkinTests` renders all skins with short, normal and long device names, checks
label bounds and size restoration, and writes individual PNGs plus light/dark
desktop contact sheets to `AudioSwitch.Tests/bin/<configuration>/net10.0-windows/osd-previews/`.
Run `dotnet test AudioSwitch.Tests --filter FullyQualifiedName~OsdSkinTests` to
regenerate them. The `device-gallery-*.png` sheets use native pixel sizes for
review; the desktop backgrounds also reveal contrast issues in translucent skins.

The default skin's PNGs can be regenerated with `tools/Build-DefaultOsd.ps1`.

## Audio Failure Diagnostics

Native failures preserve their HRESULT and operation, including unknown driver
codes and HRESULTs that .NET maps to file or argument exceptions. Default changes
are attempted once per requested role and checked against Windows afterwards.
Partial role/pair success is retained, not rolled back; failed commands refresh
the flyout and produce a throttled failure notification. Optional shell OSD
failures do not replay an already-completed audio write.

`errors.log`, in the default settings directory, contains JSON lines with local timestamps
(including the UTC offset),
operation, HRESULT, stack/cause, app version/build ID, Windows version and
architecture, runtime, process/session/thread and available device observations.
Open it in Notepad using **Open error log** in the bottom-left corner of Settings,
available from every tab.
Device IDs, names, property keys, activation results and requested values can
appear; review these before sharing logs publicly. Logging uses cached managed
observations only, never additional COM queries. Missing observations are not
invented; property failures and diagnostic-collection failures are recorded.
Logging and cleanup are best effort and cannot replace the original exception.

The log rotates at approximately 1 MiB to `errors.log.1`, retaining one backup.
Duplicate operation/device/HRESULT records are suppressed for one minute, with
the suppressed count in the next matching record. Successful retried reads emit
recovery records. Field lengths and deduplication entries are bounded.

Windows reporting no default endpoint (`0x80070490` during default lookup) is
normal and does not produce an error record. Other native failures remain logged.
Tests set `AUDIOSWITCH_LOG_DIRECTORY` to a unique directory under their output
folder's `test-logs`; the test runner and its child GUI/CLI processes each write
`errors-<process ID>.log` there instead of adding simulated failures to the user log.

Device sessions cache metadata and explicit unsupported results (`E_NOINTERFACE`,
`E_NOTIMPL`, `ERROR_NOT_SUPPORTED`) across wrapper/monitor recreation. State,
arrival/removal and property notifications invalidate these caches; restarting
also clears them. Default-role changes alone do not. Failed names display
`<Unknown name>` instead of hiding the endpoint. Other property/activation
failures have a 30-second cooldown (2 seconds for unavailable endpoints); optional
meter reads have a 5-second cooldown. Existing flyout recovery reacquires stale
monitors, including disconnected COM/RPC proxies. No automatic write retries or
Windows service restarts are performed.

Capabilities are learned lazily from normal calls, not by exhaustive startup
probing. A silent meter is valid. Hardware-support flags do not prove an interface
is unavailable: Windows may provide a software implementation, as documented for
[IAudioMeterInformation](https://learn.microsoft.com/en-us/windows/win32/api/endpointvolume/nf-endpointvolume-iaudiometerinformation-queryhardwaresupport).

## License

AudioSwitch is licensed under the [Apache License 2.0](../LICENSE). The adapted
Core Audio interop code in `AudioSwitch.Core/CoreAudio` retains its original
license notices and modification notes. Default-device switching uses an
undocumented Windows policy COM interface, isolated in `AudioSwitch.Core/CoreAudio/IPolicyConfig.cs`.
