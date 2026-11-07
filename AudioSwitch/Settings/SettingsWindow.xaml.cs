using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private readonly App app;
    private readonly AppSettings settings;
    private readonly ObservableCollection<HotkeySettings> hotkeys;
    private readonly List<Action> commitEditors = [];
    private readonly List<Action> cleanupEditors = [];
    private readonly Dictionary<string, byte[]> pendingIcons = new();
    private bool initialized;
    private readonly DispatcherTimer logTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    internal SettingsWindow(App app, AppSettings appsettings)
    {
        this.app = app;
        this.settings = appsettings;
        hotkeys = new(appsettings.Hotkeys);
        InitializeComponent();
        DataContext = appsettings;
        Height = Math.Min(600, SystemParameters.WorkArea.Height - 20);
        void UpdateLogButton() => openLog.IsEnabled = File.Exists(AudioDiagnostics.LogPath);
        UpdateLogButton();
        logTimer.Tick += (_, _) => UpdateLogButton();
        logTimer.Start();
        InitializeGeneralTab();
        InitializeDevicesTab(Direction.Playback, playbackDevices, playbackEditor);
        InitializeDevicesTab(Direction.Recording, recordingDevices, recordingEditor);
        InitializeHotkeysTab();
        InitializeOsdTab();
        InitializeAboutTab();
        app.Osd.Moved += OsdMoved;
        initialized = true;
    }

    private void ApplyClick(object sender, RoutedEventArgs e) => Save();

    private void SaveCloseClick(object sender, RoutedEventArgs e)
    {
        if (Save())
            Close();
    }

    private void CancelClick(object sender, RoutedEventArgs e) => Close();

    private void OpenLogClick(object sender, RoutedEventArgs e) => OpenErrorLog();

    private bool Save()
    {
        try
        {
            if (!int.TryParse(timeoutInput.Text, out var timeout) || timeout is < 100 or > 100000)
            {
                throw new InvalidOperationException(
                    "OSD duration must be between 100 and 100000 milliseconds."
                );
            }

            foreach (var commit in commitEditors)
            {
                commit();
            }

            settings.Hotkeys = hotkeys.ToList();
            if (settings.VolumeScroll && settings.ScrollKeys == 0)
            {
                throw new InvalidOperationException(
                    "Select a modifier for global volume scrolling."
                );
            }

            app.SaveSettings(settings.Clone(), pendingIcons);
            pendingIcons.Clear();
            if (app.IsDefaultProfile)
            {
                StartupRegistration.SetEnabled(startup.IsChecked == true);
            }

            statusText.Text = "Saved.";
            return true;
        }
        catch (Exception ex)
        {
            statusText.Text = ex.Message;
            return false;
        }
    }

    private void InitializeAboutTab()
    {
        var assembly = typeof(App).Assembly;
        aboutName.Text =
            assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
            ?? assembly.GetName().Name
            ?? "AudioSwitch";
        aboutVersion.Text =
            "Version "
            + (
                assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "Unknown"
            );
        var frames = BitmapDecoder.Create(
            new Uri("pack://application:,,,/AudioSwitch;component/Assets/AudioSwitchIcon.ico"),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad
        );
        aboutIcon.Source = frames.Frames.MaxBy(frame => frame.PixelWidth);
    }

    private void AboutNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        try
        {
            using var process = Process.Start(
                new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }
            );
        }
        catch (Exception ex)
        {
            statusText.Text = $"Could not open the link: {ex.Message}";
        }
    }

    private void InitializeGeneralTab()
    {
        theme.ItemsSource = Enum.GetValues<AppTheme>();
        deviceGroup.ItemsSource = Enum.GetValues<DeviceGroup>();
        startup.IsChecked = StartupRegistration.IsEnabled;
        startup.IsEnabled = app.IsDefaultProfile;
        scrollPicker.Initialize(settings);
    }

    private void ThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (theme.SelectedItem is AppTheme selected)
            app.ApplyTheme(selected);
    }

    private void OpenErrorLog()
    {
        try
        {
            if (!File.Exists(AudioDiagnostics.LogPath))
            {
                return;
            }

            var start = new ProcessStartInfo(
                Path.Combine(Environment.SystemDirectory, "notepad.exe")
            )
            {
                UseShellExecute = false,
            };
            start.ArgumentList.Add(AudioDiagnostics.LogPath);
            using var process = Process.Start(start);
        }
        catch (Exception ex)
        {
            AudioDiagnostics.Log.Failure("open error log", ex);
            statusText.Text = "Could not open the error log: " + ex.Message;
        }
    }

    private void TabsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!initialized || !ReferenceEquals(e.Source, tabs))
            return;
        foreach (var commit in commitEditors)
            commit();
        if (ReferenceEquals(tabs.SelectedItem, hotkeysTab))
            hotkeyGrid.Items.Refresh();
        if (ReferenceEquals(tabs.SelectedItem, osdTab))
            Preview();
        else
            app.Osd.EndPreview();
    }

    private void WindowClosed(object? sender, EventArgs e)
    {
        logTimer.Stop();
        foreach (var cleanup in cleanupEditors)
            cleanup();
        app.Osd.Moved -= OsdMoved;
        app.Osd.EndPreview();
    }
}
