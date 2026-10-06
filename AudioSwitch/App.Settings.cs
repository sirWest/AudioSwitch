using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AudioSwitch.Core.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch;

public sealed partial class App
{
    private void SettingsChanged(object sender, FileSystemEventArgs e) =>
        Dispatcher.BeginInvoke(() =>
        {
            // An open editor retains its snapshot; SettingsStore detects conflicting saves.
            if (settingsWindow is not null)
            {
                return;
            }

            RunSafely(ReloadSettings);
        });

    private void ReloadSettings()
    {
        var nextStore = new SettingsStore(settingsPath);
        var next = nextStore.Load();
        // Keep the current snapshot if Windows rejects one of the replacement hotkeys.
        shortcuts.Apply(next);
        Settings = next;
        store = nextStore;
        ApplyTheme(Settings.Theme);
        flyout.Refresh();
    }

    private static int ParseLegacyKey(string name)
    {
        if (int.TryParse(name, out var numeric))
        {
            return numeric;
        }

        name = name switch
        {
            "Return" => "Enter",
            "Capital" => "CapsLock",
            "Prior" => "PageUp",
            "Next" => "PageDown",
            "Snapshot" => "PrintScreen",
            "Oemtilde" => "Oem3",
            "Oemcomma" => "OemComma",
            _ => name,
        };
        return Enum.TryParse<Key>(name, true, out var key) ? KeyInterop.VirtualKeyFromKey(key) : 0;
    }

    internal void OpenSettings()
    {
        if (settingsWindow is not null)
        {
            settingsWindow.Activate();
            return;
        }

        RunSafely(() =>
        {
            store = new(settingsPath);
            var latest = store.Load();
            settingsWindow = new(this, latest.Clone());
            settingsWindow.Closed += (_, _) =>
            {
                settingsWindow = null;
                Osd.EndPreview();
                if (exitWithSettings)
                {
                    Shutdown();
                }
                else
                {
                    RunSafely(ReloadSettings);
                }
            };
            settingsWindow.Show();
        });
    }

    internal void SaveSettings(AppSettings settings)
    {
        settings.Validate();
        shortcuts.Apply(settings);
        try
        {
            store.Save(settings);
        }
        catch
        {
            // A conflicting or failed save must not leave unsaved shortcuts active.
            shortcuts.Apply(Settings);
            throw;
        }

        Settings = settings;
        ApplyTheme(Settings.Theme);
        iconKey = null;
        flyout.Refresh();
        if (Settings.AlwaysVisible)
        {
            flyout.Open();
        }
        else
        {
            flyout.Hide();
        }
    }

    internal void SavePosition(Window window)
    {
        Native.GetWindowRect(new WindowInteropHelper(window).Handle, out var rect);
        var positionStore = new SettingsStore(settingsPath);
        var value = positionStore.Load();
        value.FlyoutLeft = rect.Left;
        value.FlyoutTop = rect.Top;
        positionStore.Save(value);
        Settings.FlyoutLeft = rect.Left;
        Settings.FlyoutTop = rect.Top;
    }

    internal void ApplyTheme(AppTheme theme)
    {
        // WPF's supported Fluent theme mechanism; the C# setter is still marked experimental.
#pragma warning disable WPF0001
        ThemeMode = theme switch
        {
            AppTheme.Dark => ThemeMode.Dark,
            AppTheme.Light => ThemeMode.Light,
            _ => ThemeMode.System,
        };
#pragma warning restore WPF0001
    }
}
