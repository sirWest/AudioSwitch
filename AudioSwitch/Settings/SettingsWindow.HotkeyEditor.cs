using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AudioSwitch.Core.Settings;
using AudioSwitch.Input;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void EditHotkey(bool add, DataGrid grid)
    {
        if (!add && grid.SelectedItem is not HotkeySettings)
        {
            return;
        }

        var old = add ? null : (HotkeySettings)grid.SelectedItem;
        var item = new HotkeySettings
        {
            Function = old?.Function ?? HotkeyAction.NextPlaybackDevice,
            Modifiers = old?.Modifiers ?? 0,
            VirtualKey = old?.VirtualKey ?? 0,
            ShowOsd = old?.ShowOsd ?? true,
        };
        var dialog = new Window
        {
            Title = "Hotkey",
            Owner = this,
            Width = 390,
            SizeToContent = SizeToContent.Height,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        var body = new StackPanel { Margin = new Thickness(18) };
        dialog.Content = body;
        Choices(body, "Action", item, nameof(item.Function), Enum.GetValues<HotkeyAction>());
        Label(body, "Shortcut");
        var capture = new TextBox
        {
            Text = ShortcutConverter.Format(item),
            IsReadOnly = true,
            Padding = new Thickness(8),
            ToolTip = "Press a key combination",
        };
        body.Children.Add(capture);
        capture.PreviewKeyDown += (_, e) =>
        {
            e.Handled = true;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
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
        var warning = new TextBlock
        {
            Foreground = Brushes.Firebrick,
            TextWrapping = TextWrapping.Wrap,
        };
        body.Children.Add(warning);
        body.Children.Add(
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

                    if (old is not null)
                    {
                        hotkeys[hotkeys.IndexOf(old)] = item;
                    }
                    else
                    {
                        hotkeys.Add(item);
                    }

                    dialog.DialogResult = true;
                }
            )
        );
        dialog.ShowDialog();
    }
}
