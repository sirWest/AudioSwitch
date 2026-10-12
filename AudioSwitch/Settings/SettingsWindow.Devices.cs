using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private UIElement CreateDevicesTab(Direction direction)
    {
        var panel = new Grid();
        panel.ColumnDefinitions.Add(new() { Width = new GridLength(200) });
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
                    ((Image)row.Children[0]).Source = app.DeviceIcons.Get(
                        device,
                        draft.ForDevice(device.Id),
                        pendingIcons
                    );
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
            var header = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
            var reset = Button(
                "Reset device",
                () =>
                {
                    draft.Devices.RemoveAll(d => d.Id == device.Id);
                    editing = null;
                    Edit(device);
                    RefreshNames();
                    PrunePending();
                }
            );
            reset.Margin = new Thickness(8, 0, 0, 0);
            reset.VerticalAlignment = VerticalAlignment.Top;
            DockPanel.SetDock(reset, Dock.Right);
            header.Children.Add(reset);
            header.Children.Add(
                new TextBlock
                {
                    Text = device.Name,
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            );
            editor.Children.Add(header);
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
            var nameRow = new DockPanel { Margin = new Thickness(0, 3, 0, 5) };
            custom.Margin = new Thickness(0, 0, 8, 0);
            custom.VerticalAlignment = VerticalAlignment.Center;
            nameRow.Children.Add(custom);
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
            nameRow.Children.Add(name);
            editor.Children.Add(nameRow);
            editor.Children.Add(
                Check(
                    $"Default {direction.ToString().ToLowerInvariant()} device at sign-in",
                    editing,
                    nameof(editing.StartupMultimedia)
                )
            );
            if (direction == Direction.Recording)
            {
                editor.Children.Add(
                    Check(
                        "Default communications device at sign-in",
                        editing,
                        nameof(editing.StartupCommunications)
                    )
                );
            }

            AddDeviceIconEditor(editor, device, editing, Commit);
        }

        foreach (var device in app.Audio.List(direction, draft))
        {
            var row = new DockPanel();
            var icon = new Image
            {
                Source = app.DeviceIcons.Get(device, draft.ForDevice(device.Id), pendingIcons),
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
}
