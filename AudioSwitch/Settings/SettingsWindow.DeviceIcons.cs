using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using Microsoft.Win32;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void AddDeviceIconEditor(
        StackPanel editor,
        AudioDevice device,
        DeviceSettings editing,
        Action commit
    )
    {
        var tray = Check("Use device icon in tray", editing, nameof(editing.UseDeviceIconInTray));
        editor.Children.Add(tray);
        var row = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        var preview = new Image
        {
            Width = 32,
            Height = 32,
            Margin = new Thickness(0, 0, 8, 0),
        };
        AutomationProperties.SetName(preview, "Tray icon preview");
        RenderOptions.SetBitmapScalingMode(preview, BitmapScalingMode.HighQuality);
        row.Children.Add(preview);
        editor.Children.Add(row);
        var colors = new StackPanel();
        Button reset = null!;
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
            editing.UseDeviceIconInTray = tray.IsChecked == true;
            var custom = !string.IsNullOrEmpty(editing.CustomIcon);
            var useIcon = DeviceIcons.UsesDeviceIcon(editing);
            colors.IsEnabled = !useIcon;
            colors.Opacity = useIcon ? .4 : 1;
            reset.IsEnabled = custom;
            deviceIcon = app.DeviceIcons.Get(device, editing, pendingIcons);
            PreviewIcon();
        }
        row.Children.Add(
            Button(
                "Browse...",
                () =>
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
                }
            )
        );
        reset = Button(
            "Reset icon",
            () =>
            {
                editing.CustomIcon = "";
                commit();
                Refresh();
                PrunePending();
            }
        );
        row.Children.Add(reset);
        foreach (
            var slider in new[]
            {
                Slider(colors, "Hue", editing, nameof(editing.Hue), 0, 360),
                Slider(colors, "Saturation", editing, nameof(editing.Saturation), -100, 100),
                Slider(colors, "Brightness", editing, nameof(editing.Brightness), -100, 100),
            }
        )
        {
            slider.ValueChanged += (_, _) => PreviewIcon();
        }
        editor.Children.Add(colors);
        tray.Checked += (_, _) => Refresh();
        tray.Unchecked += (_, _) => Refresh();
        preview.Loaded += (_, _) =>
        {
            app.TrayStateChanged += PreviewIcon;
            PreviewIcon();
        };
        preview.Unloaded += (_, _) => app.TrayStateChanged -= PreviewIcon;
        Refresh();
    }

    private void PrunePending()
    {
        var retained = draft.Devices.Select(d => d.CustomIcon).ToHashSet(StringComparer.Ordinal);
        foreach (var name in pendingIcons.Keys.Where(n => !retained.Contains(n)).ToArray())
            pendingIcons.Remove(name);
    }
}
