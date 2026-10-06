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
    private UIElement CreateHotkeysTab()
    {
        var panel = new DockPanel();
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal };
        DockPanel.SetDock(toolbar, Dock.Bottom);
        panel.Children.Add(toolbar);
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserResizeRows = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            MinRowHeight = 40,
            ColumnHeaderHeight = 34,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            ItemsSource = hotkeys,
        };
        ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        var textStyle = new Style(typeof(TextBlock));
        textStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
        textStyle.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(6)));
        grid.Columns.Add(
            new DataGridTextColumn
            {
                Header = "Action",
                Binding = new Binding { Converter = new HotkeyDescriptionConverter(draft) },
                ElementStyle = textStyle,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            }
        );
        grid.Columns.Add(
            new DataGridTextColumn
            {
                Header = "Shortcut",
                Binding = new Binding { Converter = new ShortcutConverter() },
                ElementStyle = textStyle,
                Width = 180,
            }
        );
        void Delete()
        {
            if (grid.SelectedItem is HotkeySettings selected)
            {
                hotkeys.Remove(selected);
            }
        }
        toolbar.Children.Add(Button("Add", () => EditHotkey(true, grid)));
        var edit = Button("Edit", () => EditHotkey(false, grid));
        var duplicate = Button("Duplicate", () => EditHotkey(true, grid, duplicate: true));
        var delete = Button("Delete", Delete);
        toolbar.Children.Add(edit);
        toolbar.Children.Add(duplicate);
        toolbar.Children.Add(delete);
        void UpdateSelection()
        {
            edit.IsEnabled =
                duplicate.IsEnabled =
                delete.IsEnabled =
                    grid.SelectedItem is HotkeySettings;
        }
        grid.SelectionChanged += (_, _) => UpdateSelection();
        grid.IsVisibleChanged += (_, _) =>
        {
            if (grid.IsVisible)
            {
                foreach (var commit in commitEditors)
                {
                    commit();
                }
                grid.Items.Refresh();
            }
        };
        UpdateSelection();
        grid.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Delete)
            {
                Delete();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                EditHotkey(false, grid);
                e.Handled = true;
            }
        };
        grid.MouseDoubleClick += (_, e) =>
        {
            if (
                ItemsControl.ContainerFromElement(grid, e.OriginalSource as DependencyObject)
                is DataGridRow
            )
            {
                EditHotkey(false, grid);
            }
        };
        panel.Children.Add(grid);
        return panel;
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
