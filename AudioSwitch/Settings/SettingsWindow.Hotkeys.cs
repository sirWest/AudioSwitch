using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AudioSwitch.Core.Settings;
using AudioSwitch.Input;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void EditHotkey(bool add, DataGrid grid, bool duplicate = false)
    {
        if ((!add || duplicate) && grid.SelectedItem is not HotkeySettings)
            return;
        foreach (var commit in commitEditors)
            commit();
        var dialog = new HotkeyEditorWindow(this, app, settings, hotkeys, add, grid, duplicate);
        dialog.ShowDialog();
    }

    private void InitializeHotkeysTab()
    {
        hotkeyGrid.ItemsSource = hotkeys;
        editHotkey.IsEnabled = duplicateHotkey.IsEnabled = deleteHotkey.IsEnabled = false;
        hotkeyActionColumn.Binding = new Binding
        {
            Converter = new HotkeyDescriptionConverter(settings),
        };
        hotkeyShortcutColumn.Binding = new Binding { Converter = new ShortcutConverter() };
    }

    private void AddHotkeyClick(object sender, RoutedEventArgs e) => EditHotkey(true, hotkeyGrid);

    private void EditHotkeyClick(object sender, RoutedEventArgs e) => EditHotkey(false, hotkeyGrid);

    private void DuplicateHotkeyClick(object sender, RoutedEventArgs e) =>
        EditHotkey(true, hotkeyGrid, duplicate: true);

    private void DeleteHotkeyClick(object sender, RoutedEventArgs e) => DeleteSelectedHotkey();

    private void DeleteSelectedHotkey()
    {
        if (hotkeyGrid.SelectedItem is HotkeySettings selected)
            hotkeys.Remove(selected);
    }

    private void HotkeySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        editHotkey.IsEnabled =
            duplicateHotkey.IsEnabled =
            deleteHotkey.IsEnabled =
                hotkeyGrid.SelectedItem is HotkeySettings;
    }

    private void HotkeyPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
            DeleteSelectedHotkey();
        else if (e.Key == Key.Enter)
            EditHotkey(false, hotkeyGrid);
        else
            return;
        e.Handled = true;
    }

    private void HotkeyDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (
            e.OriginalSource is DependencyObject source
            && ItemsControl.ContainerFromElement(hotkeyGrid, source) is DataGridRow
        )
            EditHotkey(false, hotkeyGrid);
    }

    private sealed class HotkeyDescriptionConverter(AppSettings settings) : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hotkey = (HotkeySettings)value;
            var action = HotkeyPresentation.ActionName(hotkey.Function);
            var title = string.IsNullOrWhiteSpace(hotkey.Name) ? action : hotkey.Name;
            if (hotkey.Function != HotkeyAction.SelectAudioDevices)
            {
                return title == action ? title : $"{title}\n{action}";
            }
            return $"{title}\n{HotkeyPresentation.Describe(hotkey, settings)}";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => throw new NotSupportedException();
    }
}
