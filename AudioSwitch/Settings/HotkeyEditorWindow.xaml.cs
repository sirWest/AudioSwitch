using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Input;
using AudioSwitch.Shell;

namespace AudioSwitch.Settings;

internal sealed partial class HotkeyEditorWindow
{
    private readonly App app;
    private readonly AppSettings draft;
    private readonly ObservableCollection<HotkeySettings> hotkeys;
    private readonly HotkeySettings item;
    private readonly HotkeySettings? old;
    private readonly DataGrid grid;
    private readonly double availableWidth;
    private bool deviceEditorsCreated;
    private bool initialized;

    internal HotkeyEditorWindow(
        SettingsWindow owner,
        App app,
        AppSettings draft,
        ObservableCollection<HotkeySettings> hotkeys,
        bool add,
        DataGrid grid,
        bool duplicate
    )
    {
        this.app = app;
        this.draft = draft;
        this.hotkeys = hotkeys;
        this.grid = grid;
        InitializeComponent();
        old = add ? null : (HotkeySettings)grid.SelectedItem;
        var source = duplicate ? (HotkeySettings)grid.SelectedItem : old;
        item = source?.Clone() ?? new HotkeySettings { Function = HotkeyAction.NextPlaybackDevice };
        if (duplicate)
        {
            item.VirtualKey = 0;
            item.Modifiers = 0;
        }
        Native.GetWindowRect(new WindowInteropHelper(owner).Handle, out var ownerBounds);
        var work = Native
            .MonitorAt(
                new Native.Point
                {
                    X = (ownerBounds.Left + ownerBounds.Right) / 2,
                    Y = (ownerBounds.Top + ownerBounds.Bottom) / 2,
                }
            )
            .Work;
        var dpi = VisualTreeHelper.GetDpi(owner);
        availableWidth = (work.Right - work.Left) / dpi.DpiScaleX - 24;
        item.AlsoCommunications ??= draft.AlsoCommunications;
        Title = add ? "Add hotkey" : "Edit hotkey";
        Owner = owner;
        Width = Math.Min(460, availableWidth);
        MaxHeight = (work.Bottom - work.Top) / dpi.DpiScaleY - 24;
        DataContext = item;
        void CenterDialog()
        {
            if (!IsLoaded)
            {
                return;
            }
            // WPF only centers on first show; keep later content-driven resizing on-screen too.
            Native.GetWindowRect(new WindowInteropHelper(this).Handle, out var bounds);
            var width = bounds.Right - bounds.Left;
            var height = bounds.Bottom - bounds.Top;
            Native.Position(
                this,
                Math.Clamp(
                    (ownerBounds.Left + ownerBounds.Right - width) / 2,
                    work.Left,
                    Math.Max(work.Left, work.Right - width)
                ),
                Math.Clamp(
                    (ownerBounds.Top + ownerBounds.Bottom - height) / 2,
                    work.Top,
                    Math.Max(work.Top, work.Bottom - height)
                )
            );
        }
        Loaded += (_, _) => CenterDialog();
        SizeChanged += (_, _) => CenterDialog();
        name.Text = item.Name;
        capture.Text = ShortcutConverter.Format(item);
        action.ItemsSource = Enum.GetValues<HotkeyAction>()
            .Select(value => new HotkeyOption<HotkeyAction>(
                value,
                HotkeyPresentation.ActionName(value)
            ))
            .ToArray();
        action.SelectedValue = item.Function;

        initialized = true;
        UpdateAction();
    }

    private void ActionChanged(object sender, SelectionChangedEventArgs e) => UpdateAction();

    private void CancelClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private void CaptureKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Tab)
        {
            return;
        }
        e.Handled = true;
        if (
            key
            is Key.LeftCtrl
                or Key.RightCtrl
                or Key.LeftAlt
                or Key.RightAlt
                or Key.LeftShift
                or Key.RightShift
                or Key.LWin
                or Key.RWin
        )
        {
            return;
        }
        item.VirtualKey = KeyInterop.VirtualKeyFromKey(key);
        item.Modifiers = (uint)Keyboard.Modifiers;
        capture.Text = ShortcutConverter.Format(item);
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (item.VirtualKey == 0)
        {
            warning.Text = "Choose a shortcut.";
            return;
        }
        if (
            hotkeys.Any(h =>
                h != old && h.VirtualKey == item.VirtualKey && h.Modifiers == item.Modifiers
            )
        )
        {
            warning.Text = "This shortcut is already assigned.";
            return;
        }
        if (
            item.Function == HotkeyAction.SelectAudioDevices
            && item.Playback.DeviceId is null
            && item.Recording.DeviceId is null
            && !item.Playback.MuteOthers
            && !item.Recording.MuteOthers
        )
        {
            warning.Text = "Choose a device or a mute action.";
            return;
        }
        item.Name = name.Text.Trim();
        if (old is not null)
        {
            hotkeys[hotkeys.IndexOf(old)] = item;
        }
        else
        {
            hotkeys.Add(item);
        }
        grid.SelectedItem = item;
        grid.ScrollIntoView(item);
        DialogResult = true;
    }

    private void UpdateAction()
    {
        if (!initialized || action.SelectedValue is not HotkeyAction selected)
            return;
        item.Function = selected;
        var show = item.Function == HotkeyAction.SelectAudioDevices;
        if (show && !deviceEditorsCreated)
        {
            InitializeHotkeyDeviceEditor(playback, "Playback", Direction.Playback, item.Playback);
            InitializeHotkeyDeviceEditor(
                recording,
                "Recording",
                Direction.Recording,
                item.Recording
            );
            deviceEditorsCreated = true;
        }
        devices.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        communications.Visibility = devices.Visibility;
        deviceColumn.Width = show ? new GridLength(1.2, GridUnitType.Star) : new GridLength(0);
        Width = Math.Min(show ? 820 : 460, availableWidth);
    }

    private void InitializeHotkeyDeviceEditor(
        HotkeyDeviceEditor editor,
        string title,
        Direction direction,
        HotkeyDeviceSettings options
    )
    {
        editor.DataContext = options;
        editor.heading.Text = title + " device";
        var deviceChoice = editor.deviceChoice;
        var muteChoice = editor.muteChoice;
        var mutePanel = editor.mutePanel;
        var excluded = editor.excluded;
        var others = editor.others;
        AutomationProperties.SetName(deviceChoice, title + " device");
        IReadOnlyList<AudioDevice> available;
        try
        {
            available = app.Audio.List(direction, draft);
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioDiagnostics.Log.Failure($"list {direction} hotkey choices", ex);
            available = [];
        }
        var choices = new List<HotkeyOption<string>> { new("", "Leave unchanged") };
        choices.AddRange(
            available.Select(device => new HotkeyOption<string>(
                device.Id,
                device.DisplayName(draft)
            ))
        );
        if (options.DeviceId is not null && available.All(d => d.Id != options.DeviceId))
        {
            choices.Add(
                new(
                    options.DeviceId,
                    HotkeyPresentation.DeviceName(options, draft) + " (unavailable)"
                )
            );
        }
        deviceChoice.ItemsSource = choices;
        deviceChoice.SelectedValue = options.DeviceId ?? "";
        muteChoice.ItemsSource = Enum.GetValues<HotkeyMuteAction>()
            .Select(value => new HotkeyOption<HotkeyMuteAction>(
                value,
                HotkeyPresentation.MuteName(value)
            ))
            .ToArray();
        muteChoice.SelectedValue = options.MuteAction;
        muteChoice.SelectionChanged += (_, _) =>
            options.MuteAction = (HotkeyMuteAction)muteChoice.SelectedValue;
        void UpdateDevice()
        {
            var id = (string)deviceChoice.SelectedValue;
            options.DeviceId = id.Length == 0 ? null : id;
            var selected = available.FirstOrDefault(d => d.Id == id);
            if (selected is not null)
            {
                options.DeviceName = selected.DisplayName(draft);
            }
            var isExcluded = draft.ForDevice(id)?.ExcludeFromHotkeyMute == true;
            mutePanel.Visibility =
                options.DeviceId is not null && !isExcluded
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            excluded.Visibility =
                options.DeviceId is not null && isExcluded
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            others.Content = options.DeviceId is null
                ? $"Mute all {title.ToLowerInvariant()} devices"
                : $"Mute other {title.ToLowerInvariant()} devices";
        }
        deviceChoice.SelectionChanged += (_, _) => UpdateDevice();
        UpdateDevice();
    }

    private sealed record HotkeyOption<T>(T Value, string Label)
    {
        public override string ToString() => Label;
    }
}
