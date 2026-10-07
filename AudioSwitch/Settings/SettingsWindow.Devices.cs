using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using AudioSwitch.Shell;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private UIElement CreateDevicesTab(Direction direction)
    {
        var panel = new Grid();
        panel.ColumnDefinitions.Add(new() { Width = new GridLength(220) });
        panel.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        var list = new ListBox
        {
            Margin = new Thickness(0, 0, 14, 0),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };
        panel.Children.Add(list);
        ScrollViewer.SetHorizontalScrollBarVisibility(list, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(list, ScrollBarVisibility.Auto);
        var editor = new StackPanel();
        var editorScroll = new ScrollViewer
        {
            Content = editor,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        Grid.SetColumn(editorScroll, 1);
        panel.Children.Add(editorScroll);
        DeviceSettings? editing = null;
        var refreshing = false;
        void RefreshNames()
        {
            var selection = list.SelectedItem;
            var items = list
                .Items.Cast<ListBoxItem>()
                .ToDictionary(item => ((AudioDevice)item.Tag).Id);
            // Reordering raises SelectionChanged; suppress commits until selection is restored.
            refreshing = true;
            try
            {
                list.Items.Clear();
                foreach (
                    var device in AudioDevice.SortByDisplayName(
                        items.Values.Select(item => (AudioDevice)item.Tag),
                        draft
                    )
                )
                {
                    var item = items[device.Id];
                    var row = (DockPanel)item.Content;
                    ((TextBlock)row.Children[1]).Text = device.DisplayName(draft);
                    list.Items.Add(item);
                }

                list.SelectedItem = selection;
            }
            finally
            {
                refreshing = false;
            }
        }

        void Commit()
        {
            if (editing is null)
            {
                return;
            }

            // Each direction can have only one startup default for each Windows audio role.
            foreach (
                var preference in draft.Devices.Where(d =>
                    d.Direction == direction && d.Id != editing.Id
                )
            )
            {
                if (editing.StartupMultimedia)
                {
                    preference.StartupMultimedia = false;
                }
                if (editing.StartupCommunications)
                {
                    preference.StartupCommunications = false;
                }
            }

            draft.Devices.RemoveAll(d => d.Id == editing.Id);
            draft.Devices.Add(editing);
            RefreshNames();
        }

        commitEditors.Add(Commit);
        void Edit(AudioDevice device)
        {
            editing =
                draft.ForDevice(device.Id)
                ?? new DeviceSettings { Id = device.Id, Direction = direction };
            editing.Direction = direction;
            editor.Children.Clear();
            Label(editor, device.Name);
            editor.Children.Add(
                Check("Hide from device list and cycling", editing, nameof(editing.Hidden))
            );
            var custom = Check("Use custom name", editing, nameof(editing.UseCustomName));
            editor.Children.Add(
                Check(
                    "Exclude from hotkey mute/unmute",
                    editing,
                    nameof(editing.ExcludeFromHotkeyMute)
                )
            );
            editor.Children.Add(custom);
            var name = new TextBox { Margin = new Thickness(0, 3, 0, 5) };
            name.SetBinding(
                TextBox.TextProperty,
                new Binding(nameof(editing.CustomName))
                {
                    Source = editing,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                }
            );
            name.SetBinding(
                IsEnabledProperty,
                new Binding(nameof(CheckBox.IsChecked)) { Source = custom }
            );
            editor.Children.Add(name);
            editor.Children.Add(
                Check(
                    "Default multimedia device at sign-in",
                    editing,
                    nameof(editing.StartupMultimedia)
                )
            );
            editor.Children.Add(
                Check(
                    "Default communications device at sign-in",
                    editing,
                    nameof(editing.StartupCommunications)
                )
            );
            AddTrayIconEditor(editor, editing);
            var buttons = new WrapPanel();
            editor.Children.Add(buttons);
            buttons.Children.Add(
                Button(
                    "Save device",
                    () =>
                    {
                        Commit();
                        statusText.Text = "Device preferences staged. Apply to save.";
                    }
                )
            );
            buttons.Children.Add(
                Button(
                    "Reset device",
                    () =>
                    {
                        draft.Devices.RemoveAll(d => d.Id == device.Id);
                        editing = null;
                        Edit(device);
                        RefreshNames();
                    }
                )
            );
        }

        foreach (var device in app.Audio.List(direction, draft))
        {
            var row = new DockPanel();
            var icon = new Image
            {
                Source = Native.DeviceIcon(device.IconPath),
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 0, 6, 0),
            };
            row.Children.Add(icon);
            row.Children.Add(
                new TextBlock
                {
                    Text = device.DisplayName(draft),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            );
            list.Items.Add(
                new ListBoxItem
                {
                    Content = row,
                    Tag = device,
                    Padding = new Thickness(4),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                }
            );
        }

        list.SelectionChanged += (_, _) =>
        {
            if (refreshing)
            {
                return;
            }

            Commit();
            if (list.SelectedItem is ListBoxItem { Tag: AudioDevice device })
            {
                Edit(device);
            }
        };
        list.IsVisibleChanged += (_, _) =>
        {
            if (list.IsVisible)
            {
                RefreshNames();
            }
        };
        if (list.Items.Count > 0)
        {
            list.SelectedIndex = 0;
        }
        else
        {
            Label(editor, "No active devices");
        }

        return panel;
    }

    private static void AddTrayIconEditor(StackPanel editor, DeviceSettings editing)
    {
        Label(editor, "Tray icon color");
        var previews = new StackPanel { Orientation = Orientation.Horizontal, Height = 32 };
        var previewAssets = new[]
        {
            "0.png",
            "0-25.png",
            "25-50.png",
            "50-75.png",
            "75-100.png",
            "mute.png",
        };
        foreach (var asset in previewAssets)
        {
            var preview = new Image
            {
                Width = 24,
                Height = 24,
                Margin = new Thickness(0, 0, 8, 0),
                ToolTip = asset == "mute.png" ? "Muted" : asset.Replace(".png", "%"),
                Stretch = System.Windows.Media.Stretch.Uniform,
            };
            System.Windows.Media.RenderOptions.SetBitmapScalingMode(
                preview,
                System.Windows.Media.BitmapScalingMode.HighQuality
            );
            System.Windows.Automation.AutomationProperties.SetName(
                preview,
                $"Tray icon {preview.ToolTip}"
            );
            previews.Children.Add(preview);
        }

        editor.Children.Add(previews);
        void PreviewIcon()
        {
            for (var i = 0; i < previewAssets.Length; i++)
            {
                ((Image)previews.Children[i]).Source = Images.Tint(
                    Images.TrayAsset(previewAssets[i]),
                    editing
                );
            }
        }
        foreach (
            var slider in new[]
            {
                Slider(editor, "Hue", editing, nameof(editing.Hue), 0, 360),
                Slider(editor, "Saturation", editing, nameof(editing.Saturation), -100, 100),
                Slider(editor, "Brightness", editing, nameof(editing.Brightness), -100, 100),
            }
        )
        {
            slider.ValueChanged += (_, _) => PreviewIcon();
        }

        PreviewIcon();
    }
}
