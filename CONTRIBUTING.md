# Contributing to AudioSwitch

## Getting Started

Install the .NET 10 SDK on Windows, open `AudioSwitch.sln`, and build the solution.
Use an isolated profile while developing the UI:

```powershell
dotnet run --project AudioSwitch -- --settings --config ./artifacts/development.json --exit-with-settings
```

This leaves the normal profile and sign-in registration alone. Audio commands
still operate on the current Windows audio session.

## Code Organization

- `AudioSwitch/App.cs` owns startup, shutdown and error handling. `App.Commands.cs` connects tray and hotkey actions; `App.Settings.cs` owns settings reloads and saves.
- `AudioSwitch/Flyout` contains the device window, its styles, volume thumb and VU meter. The window's partial files separate layout, input and audio subscriptions from its show/hide lifecycle.
- `AudioSwitch/Settings` contains the settings window and its tabs, hotkey editor and modifier picker. All tabs edit one draft; Apply commits it through the app.
- `AudioSwitch/Osd` contains the OSD window, rendering and skin lookup.
- `AudioSwitch/Input` contains global shortcuts and their display converter.
- `AudioSwitch/Shell` contains the tray icon, sign-in registration and native window integration. Native declarations, backdrops, positioning and animation are grouped in separate partial files.
- `AudioSwitch/Imaging` contains shared image loading and tray icon rendering.
- `AudioSwitch.Core/Audio` exposes device operations, endpoint monitoring and audio models.
- `AudioSwitch.Core/Settings` contains settings models, validation, atomic persistence and legacy XML migration.
- `AudioSwitch.Core/CoreAudio` owns native COM interfaces and their lifetime wrappers, including default-device policy. Keep vtable order, marshaling and license notices intact.
- `AudioSwitch.Core/Placement` contains display-independent window geometry.
- `AudioSwitch.Cli/Program.cs` loads the profile and translates errors to exit codes. Its partial files group command dispatch, argument parsing and legacy aliases.
- `AudioSwitch.Tests` groups core tests, GUI tests, Windows integration tests and test attributes into corresponding folders.

UI work belongs in the WPF project; shared audio and persistence code belongs in
Core. Keep COM ownership explicit, unsubscribe callbacks before releasing native
objects, and marshal UI changes back to the WPF dispatcher. Avoid adding work to
the low-level mouse hook; defer it to the dispatcher as the existing code does.

## Style and Validation

Follow `.editorconfig`: four-space C# indentation, braces around control-flow
bodies, one statement per line, and conventional C# names. Keep simple properties
compact. Use comments for lifetime rules, Windows quirks and non-obvious intent.
Keep changes focused and add regression tests when behavior changes. Use short methods
to name meaningful steps, not to wrap single expressions without adding clarity.
Partial files group responsibilities of one window or entry point; they are not separate services.

CSharpier is pinned in `.config/dotnet-tools.json`. Rider's CSharpier plugin uses
that local tool, so format-on-save and command-line formatting agree. Restore it
once after cloning, then format or check the source:

```powershell
dotnet tool restore
dotnet csharpier format .
dotnet csharpier check .
dotnet build AudioSwitch.sln -c Release
dotnet test AudioSwitch.sln -c Release
```

The default tests cover settings recovery, concurrent saves, migration, placement,
interop layout and CLI help. The additional checks below are opt-in:

```powershell
# Endpoint enumeration, volume reads, monitor disposal, hotkey registration and CLI JSON.
$env:AUDIOSWITCH_TEST_SYSTEM = '1'

# Separate temporary WPF profile; exercise every settings tab and OSD skin.
$env:AUDIOSWITCH_TEST_UI = '1'

dotnet test AudioSwitch.sln -c Release
```

UI automation requires an interactive Windows desktop. Tests that change audio
require `AUDIOSWITCH_TEST_MUTATIONS=1`; default-device switching additionally uses
`AUDIOSWITCH_TEST_SWITCHING=1` and requires a local session with two playback
endpoints. Restoration happens in `finally` blocks, but cannot be guaranteed if
hardware disappears or the test process is terminated. Enable only the flags you
intend to use and remove them from your shell afterwards.

`tools/Capture-Window.ps1` captures a selected application window for visual
inspection. Build output, screenshots and local profiles belong in `artifacts`.

## Pull Requests

Add user-visible development changes to [Coming in v3.1](README.md#coming-in-v31)
as they are implemented. Keep unreleased behavior distinct from v3.0 features.
When v3.1 ships, rename that section to "What's New in v3.1", remove the development
notice, and update version labels and links in the README and technical reference.

Describe the problem, resulting behavior and tests performed. For UI changes,
include screenshots of the affected states. For audio or shell integration,
include the Windows version and relevant device, session or monitor details.
Keep generated output and personal IDE settings out of commits.

For a release, update `Directory.Build.props` and the application manifest,
then publish the main project with the `Installer` profile (or run
`tools/Publish.ps1 -BuildInstaller`). Smoke-test setup, upgrading,
startup toggling and uninstall on a separate Windows test profile or machine.
Do not change the JSON schema version unless the settings format changes.
