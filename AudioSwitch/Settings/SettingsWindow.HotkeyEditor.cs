using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using AudioSwitch.Core.Settings;
using AudioSwitch.Input;
using AudioSwitch.Shell;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void EditHotkey(bool add, DataGrid grid, bool duplicate = false)
    {
        if ((!add || duplicate) && grid.SelectedItem is not HotkeySettings)
        {
            return;
        }
        foreach (var commit in commitEditors)
        {
            commit();
        }
        var old = add ? null : (HotkeySettings)grid.SelectedItem;
        var source = duplicate ? (HotkeySettings)grid.SelectedItem : old;
        var item =
            source?.Clone() ?? new HotkeySettings { Function = HotkeyAction.NextPlaybackDevice };
        if (duplicate)
        {
            item.VirtualKey = 0;
            item.Modifiers = 0;
        }
        Native.GetWindowRect(new WindowInteropHelper(this).Handle, out var ownerBounds);
        var work = Native
            .MonitorAt(
                new Native.Point
                {
                    X = (ownerBounds.Left + ownerBounds.Right) / 2,
                    Y = (ownerBounds.Top + ownerBounds.Bottom) / 2,
                }
            )
            .Work;
        var dpi = VisualTreeHelper.GetDpi(this);
        var availableWidth = (work.Right - work.Left) / dpi.DpiScaleX - 24;
        var dialog = new Window
        {
            Title = add ? "Add hotkey" : "Edit hotkey",
            Owner = this,
            Width = Math.Min(460, availableWidth),
            SizeToContent = SizeToContent.Height,
            MaxHeight = (work.Bottom - work.Top) / dpi.DpiScaleY - 24,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            UseLayoutRounding = true,
        };
        dialog.SetResourceReference(BackgroundProperty, "ApplicationBackgroundBrush");
        dialog.SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
        void CenterDialog()
        {
            if (!dialog.IsLoaded)
            {
                return;
            }
            // WPF only centers on first show; keep later content-driven resizing on-screen too.
            Native.GetWindowRect(new WindowInteropHelper(dialog).Handle, out var bounds);
            var width = bounds.Right - bounds.Left;
            var height = bounds.Bottom - bounds.Top;
            Native.Position(
                dialog,
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
        dialog.Loaded += (_, _) => CenterDialog();
        dialog.SizeChanged += (_, _) => CenterDialog();
        var root = new DockPanel { Margin = new Thickness(18) };
        dialog.Content = root;
        var footer = new StackPanel();
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);
        var warning = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
        };
        warning.SetResourceReference(TextBlock.ForegroundProperty, "SystemFillColorCriticalBrush");
        footer.Children.Add(warning);
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        footer.Children.Add(buttons);
        var columns = new Grid();
        columns.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        var deviceColumn = new ColumnDefinition { Width = new GridLength(0) };
        columns.ColumnDefinitions.Add(deviceColumn);
        var body = new StackPanel();
        columns.Children.Add(body);
        root.Children.Add(
            new ScrollViewer
            {
                Content = columns,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            }
        );
        Label(body, "Name (optional)");
        var name = new TextBox { Text = item.Name, MaxLength = 100 };
        AutomationProperties.SetName(name, "Name (optional)");
        body.Children.Add(name);
        var action = HotkeyChoice(
            body,
            "Action",
            Enum.GetValues<HotkeyAction>(),
            item.Function,
            HotkeyPresentation.ActionName
        );
        Label(body, "Shortcut");
        var capture = new TextBox
        {
            Text = ShortcutConverter.Format(item),
            IsReadOnly = true,
            Padding = new Thickness(8),
            ToolTip = "Press a key combination",
        };
        AutomationProperties.SetName(capture, "Shortcut");
        body.Children.Add(capture);
        capture.PreviewKeyDown += (_, e) =>
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
        };
        body.Children.Add(Check("Show OSD", item, nameof(item.ShowOsd)));
        item.AlsoCommunications ??= draft.AlsoCommunications;
        const string communicationsLabel = "Also change the communications default";
        var communications = Check(communicationsLabel, item, nameof(item.AlsoCommunications));
        communications.Content = new TextBlock
        {
            Text = communicationsLabel,
            TextWrapping = TextWrapping.Wrap,
        };
        AutomationProperties.SetName(communications, communicationsLabel);
        body.Children.Add(communications);
        var devices = new StackPanel { Margin = new Thickness(24, 0, 12, 0) };
        Grid.SetColumn(devices, 1);
        columns.Children.Add(devices);
        var deviceEditorsCreated = false;
        void UpdateAction()
        {
            item.Function = (HotkeyAction)action.SelectedValue;
            var show = item.Function == HotkeyAction.SelectAudioDevices;
            if (show && !deviceEditorsCreated)
            {
                AddHotkeyDeviceEditor(
                    devices,
                    "Playback",
                    Core.Audio.Direction.Playback,
                    item.Playback
                );
                AddHotkeyDeviceEditor(
                    devices,
                    "Recording",
                    Core.Audio.Direction.Recording,
                    item.Recording
                );
                deviceEditorsCreated = true;
            }
            devices.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            communications.Visibility = devices.Visibility;
            deviceColumn.Width = show ? new GridLength(1.2, GridUnitType.Star) : new GridLength(0);
            dialog.Width = Math.Min(show ? 820 : 460, availableWidth);
        }
        action.SelectionChanged += (_, _) => UpdateAction();
        UpdateAction();
        var cancel = Button("Cancel", () => dialog.DialogResult = false);
        cancel.IsCancel = true;
        buttons.Children.Add(cancel);
        buttons.Children.Add(
            Button(
                "Save",
                () =>
                {
                    if (item.VirtualKey == 0)
                    {
                        warning.Text = "Choose a shortcut.";
                        return;
                    }
                    if (
                        hotkeys.Any(h =>
                            h != old
                            && h.VirtualKey == item.VirtualKey
                            && h.Modifiers == item.Modifiers
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
                    dialog.DialogResult = true;
                }
            )
        );
        dialog.ShowDialog();
    }

    private sealed record HotkeyOption<T>(T Value, string Label)
    {
        public override string ToString() => Label;
    }

    private static ComboBox HotkeyChoice<T>(
        Panel panel,
        string label,
        IEnumerable<T> values,
        T selected,
        Func<T, string> display
    )
    {
        Label(panel, label);
        var itemStyle = new Style(typeof(ComboBoxItem));
        itemStyle.Setters.Add(new Setter(AutomationProperties.NameProperty, new Binding("Label")));
        itemStyle.Setters.Add(new Setter(ToolTipProperty, new Binding("Label")));
        var combo = new ComboBox
        {
            ItemContainerStyle = itemStyle,
            ItemsSource = values
                .Select(value => new HotkeyOption<T>(value, display(value)))
                .ToArray(),
            DisplayMemberPath = "Label",
            SelectedValuePath = "Value",
            SelectedValue = selected,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 4),
        };
        AutomationProperties.SetName(combo, label);
        panel.Children.Add(combo);
        return combo;
    }
}
