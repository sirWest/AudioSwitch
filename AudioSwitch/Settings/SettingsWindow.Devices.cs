using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using Microsoft.Win32;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private Action ConnectDeviceIconEditor(
        DeviceEditor editor,
        AudioDevice device,
        DeviceSettings editing,
        Action commit
    )
    {
        var tray = editor.tray;
        var preview = editor.preview;
        var colors = editor.colors;
        var reset = editor.resetIcon;
        var deviceIcon = app.DeviceIcons.Get(device, editing, pendingIcons);
        void PreviewIcon()
        {
            var state = app.TrayState;
            var muted = state?.Muted ?? true;
            preview.Source = DeviceIcons.UsesDeviceIcon(editing)
                ? Images.DeviceTray(deviceIcon, muted, 64)
                : Images.Tint(
                    Images.Resize(
                        Images.TrayAsset(Images.VolumeAsset(state?.Volume ?? 0, muted)),
                        64
                    ),
                    editing
                );
            preview.ToolTip =
                state is null ? "No default audio device"
                : muted ? "Muted tray icon"
                : $"Tray icon at {state.Volume:P0}";
        }
        void Refresh()
        {
            var custom = !string.IsNullOrEmpty(editing.CustomIcon);
            var useIcon = DeviceIcons.UsesDeviceIcon(editing);
            colors.IsEnabled = !useIcon;
            colors.Opacity = useIcon ? .4 : 1;
            reset.IsEnabled = custom;
            deviceIcon = app.DeviceIcons.Get(device, editing, pendingIcons);
            PreviewIcon();
        }
        RoutedEventHandler browse = (_, _) =>
        {
            var picker = new OpenFileDialog
            {
                Title = "Choose device icon",
                Filter =
                    "Icons and images|*.ico;*.png;*.jpg;*.jpeg;*.jfif;*.bmp;*.dib;*.gif;*.tif;*.tiff;*.wdp;*.hdp|All files|*.*",
                CheckFileExists = true,
            };
            if (picker.ShowDialog(this) != true)
                return;
            try
            {
                var bytes = DeviceIcons.Import(picker.FileName);
                var name = DeviceIconStore.Name(bytes);
                pendingIcons[name] = bytes;
                editing.CustomIcon = name;
                commit();
                Refresh();
                statusText.Text = "Icon staged. Apply to save.";
                PrunePending();
            }
            catch (Exception ex)
            {
                statusText.Text = $"Could not import icon: {ex.Message}";
            }
        };
        RoutedEventHandler resetClick = (_, _) =>
        {
            editing.CustomIcon = "";
            commit();
            Refresh();
            PrunePending();
        };
        RoutedPropertyChangedEventHandler<double> valueChanged = (_, _) => PreviewIcon();
        RoutedEventHandler refresh = (_, _) => Refresh();
        RoutedEventHandler loaded = (_, _) =>
        {
            app.TrayStateChanged -= PreviewIcon;
            app.TrayStateChanged += PreviewIcon;
            PreviewIcon();
        };
        RoutedEventHandler unloaded = (_, _) => app.TrayStateChanged -= PreviewIcon;
        editor.browseIcon.Click += browse;
        reset.Click += resetClick;
        editor.hue.ValueChanged += valueChanged;
        editor.saturation.ValueChanged += valueChanged;
        editor.brightness.ValueChanged += valueChanged;
        tray.Checked += refresh;
        tray.Unchecked += refresh;
        preview.Loaded += loaded;
        preview.Unloaded += unloaded;
        if (preview.IsLoaded)
            app.TrayStateChanged += PreviewIcon;
        Refresh();
        return () =>
        {
            editor.browseIcon.Click -= browse;
            reset.Click -= resetClick;
            editor.hue.ValueChanged -= valueChanged;
            editor.saturation.ValueChanged -= valueChanged;
            editor.brightness.ValueChanged -= valueChanged;
            tray.Checked -= refresh;
            tray.Unchecked -= refresh;
            preview.Loaded -= loaded;
            preview.Unloaded -= unloaded;
            app.TrayStateChanged -= PreviewIcon;
        };
    }

    private void PrunePending()
    {
        var retained = settings.Devices.Select(d => d.CustomIcon).ToHashSet(StringComparer.Ordinal);
        foreach (var name in pendingIcons.Keys.Where(n => !retained.Contains(n)).ToArray())
            pendingIcons.Remove(name);
    }

    private sealed record DeviceRow(AudioDevice Device, string Name, ImageSource Icon);

    private void InitializeDevicesTab(Direction direction, ListBox list, DeviceEditor editor)
    {
        var devices = app.Audio.List(direction, settings).ToArray();
        DeviceSettings? editing = null;
        var refreshing = false;
        void RefreshNames()
        {
            var selectedId = (list.SelectedItem as DeviceRow)?.Device.Id;
            refreshing = true;
            try
            {
                list.ItemsSource = AudioDevice
                    .SortByDisplayName(devices, settings)
                    .Select(device => new DeviceRow(
                        device,
                        device.DisplayName(settings),
                        app.DeviceIcons.Get(device, settings.ForDevice(device.Id), pendingIcons)
                    ))
                    .ToArray();
                list.SelectedItem = list
                    .Items.Cast<DeviceRow>()
                    .FirstOrDefault(row => row.Device.Id == selectedId);
            }
            finally
            {
                refreshing = false;
            }
        }
        void Commit()
        {
            if (editing is null)
                return;
            foreach (
                var preference in settings.Devices.Where(d =>
                    d.Direction == direction && d.Id != editing.Id
                )
            )
            {
                if (editing.StartupMultimedia)
                    preference.StartupMultimedia = false;
                if (editing.StartupCommunications)
                    preference.StartupCommunications = false;
            }
            settings.Devices.RemoveAll(d => d.Id == editing.Id);
            settings.Devices.Add(editing);
            RefreshNames();
        }
        commitEditors.Add(Commit);
        Action? detachIconEditor = null;
        cleanupEditors.Add(() => detachIconEditor?.Invoke());
        void Edit(AudioDevice device)
        {
            detachIconEditor?.Invoke();
            editing =
                settings.ForDevice(device.Id)
                ?? new DeviceSettings { Id = device.Id, Direction = direction };
            editing.Direction = direction;
            editor.DataContext = editing;
            editor.deviceName.Text = device.Name;
            detachIconEditor = ConnectDeviceIconEditor(editor, device, editing, Commit);
        }
        editor.startupDefault.Content =
            $"Default {direction.ToString().ToLowerInvariant()} device at sign-in";
        editor.startupCommunications.Visibility =
            direction == Direction.Recording ? Visibility.Visible : Visibility.Collapsed;
        editor.resetDevice.Click += (_, _) =>
        {
            if (list.SelectedItem is not DeviceRow row)
                return;
            settings.Devices.RemoveAll(d => d.Id == row.Device.Id);
            editing = null;
            Edit(row.Device);
            RefreshNames();
            PrunePending();
        };
        list.SelectionChanged += (_, _) =>
        {
            if (refreshing)
                return;
            var selected = list.SelectedItem as DeviceRow;
            Commit();
            if (selected is not null)
                Edit(selected.Device);
        };
        RefreshNames();
        if (list.Items.Count > 0)
            list.SelectedIndex = 0;
        else
        {
            editor.fields.Visibility = Visibility.Collapsed;
            editor.emptyMessage.Visibility = Visibility.Visible;
        }
    }
}
