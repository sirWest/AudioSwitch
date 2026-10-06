using System.IO;
using System.Windows;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Flyout;
using AudioSwitch.Input;
using AudioSwitch.Osd;
using AudioSwitch.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch;

public sealed partial class App : Application
{
    private SettingsStore store = new();
    internal AppSettings Settings { get; private set; } = new();
    internal AudioService Audio { get; private set; } = null!;
    internal OsdWindow Osd { get; private set; } = null!;

    private TrayIcon tray = null!;
    private Shortcuts shortcuts = null!;
    private FlyoutWindow flyout = null!;
    private AudioRefreshQueue? audioRefresh;
    private SettingsWindow? settingsWindow;
    private FileSystemWatcher? watcher;
    private string? iconKey;
    private string settingsPath = SettingsStore.DefaultPath;
    internal bool IsDefaultProfile => settingsPath == SettingsStore.DefaultPath;

    private bool exitWithSettings;
    private DateTime closedFromTray;
    private DateTime lastScroll;

    [STAThread]
    public static int Main(string[] args)
    {
        var configIndex = Array.IndexOf(args, "--config");
        if (configIndex >= 0 && configIndex + 1 >= args.Length)
        {
            MessageBox.Show("--config requires a file path.", "AudioSwitch");
            return 2;
        }

        var path =
            configIndex >= 0 ? Path.GetFullPath(args[configIndex + 1]) : SettingsStore.DefaultPath;
        var instanceKey = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(path.ToUpperInvariant())
            )
        );
        var mutexName = string.Equals(
            path,
            SettingsStore.DefaultPath,
            StringComparison.OrdinalIgnoreCase
        )
            ? "Local\\AudioSwitch.Tray"
            : "Local\\AudioSwitch.Tray." + instanceKey;
        using var singleton = new Mutex(true, mutexName, out var first);
        if (!first)
        {
            return 0;
        }

        var app = new App
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown,
            settingsPath = path,
            store = new SettingsStore(path),
        };
        app.Startup += (_, _) => app.Start(args);
        app.DispatcherUnhandledException += (_, e) =>
        {
            app.Report(e.Exception);
            e.Handled = true;
        };
        return app.Run();
    }

    private void Start(string[] args)
    {
        try
        {
            exitWithSettings = args.Contains("--exit-with-settings");
            Settings = store.Load();
            if (
                settingsPath == SettingsStore.DefaultPath
                && !File.Exists(store.FilePath)
                && File.Exists(LegacySettings.DefaultPath)
            )
            {
                Settings = LegacySettings.Import(LegacySettings.DefaultPath, ParseLegacyKey);
                store.Save(Settings);
            }

            ApplyTheme(Settings.Theme);

            Audio = new();
            Osd = new();
            tray = new();
            shortcuts = new(tray, Dispatcher);
            flyout = new(this);
            SubscribeTrayEvents();
            try
            {
                shortcuts.Apply(Settings);
            }
            catch (Exception ex)
            {
                Report(ex);
            }

            audioRefresh = new AudioRefreshQueue(Dispatcher, () => RunSafely(flyout.Refresh));
            Audio.Changed += _ => audioRefresh.Request();
            flyout.Refresh();
            Directory.CreateDirectory(Path.GetDirectoryName(store.FilePath)!);
            watcher = new(Path.GetDirectoryName(store.FilePath)!, Path.GetFileName(store.FilePath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true,
            };
            watcher.Changed += SettingsChanged;
            watcher.Created += SettingsChanged;
            watcher.Renamed += SettingsChanged;
            if (args.Contains("/startup") || args.Contains("--startup"))
            {
                RunSafely(() =>
                {
                    Audio.ApplyStartup(Settings);
                    tray.Notify("Startup audio preferences applied.");
                });
            }

            if (args.Contains("--settings"))
            {
                OpenSettings();
            }

            if (args.Contains("--show") || Settings.AlwaysVisible)
            {
                flyout.Open();
            }

            if (store.RecoveryNotice is { } notice)
            {
                tray.Notify(notice);
            }
        }
        catch (Exception ex)
        {
            Report(ex);
            Shutdown(1);
        }
    }

    internal void RunSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (AudioUnavailableException.IsUnavailable(ex))
        {
            // Hotkeys and clicks can race endpoint-removal notifications.
            try
            {
                flyout.Refresh();
            }
            catch (Exception refreshError)
            {
                Report(refreshError);
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            // A role switch may have partially succeeded. Read Windows' actual state.
            audioRefresh?.Request();
            Report(ex);
        }
        catch (Exception ex)
        {
            Report(ex);
        }
    }

    private void Report(Exception ex)
    {
        try
        {
            var folder = Path.GetDirectoryName(SettingsStore.DefaultPath)!;
            Directory.CreateDirectory(folder);
            File.AppendAllText(
                Path.Combine(folder, "errors.log"),
                $"{DateTimeOffset.Now:O} {ex}\n"
            );
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        tray?.Notify(ex.Message);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        watcher?.Dispose();
        audioRefresh?.Dispose();
        flyout?.Dispose();
        shortcuts?.Dispose();
        tray?.Dispose();
        Audio?.Dispose();
        Osd?.Close();
        base.OnExit(e);
    }
}
