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
        var toolbar = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Bottom);
        panel.Children.Add(toolbar);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        DockPanel.SetDock(buttons, Dock.Left);
        toolbar.Children.Add(buttons);
        var pager = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        toolbar.Children.Add(pager);
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserResizeRows = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            RowHeight = 36,
            ColumnHeaderHeight = 34,
            HeadersVisibility = DataGridHeadersVisibility.Column,
        };
        ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);
        var page = 0;
        const int pageSize = 8;
        var pageLabel = new TextBlock
        {
            Width = 64,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Button previous = null!,
            next = null!;
        void RefreshPage()
        {
            var pages = Math.Max(1, (hotkeys.Count + pageSize - 1) / pageSize);
            page = Math.Clamp(page, 0, pages - 1);
            var selected = grid.SelectedItem;
            grid.ItemsSource = hotkeys.Skip(page * pageSize).Take(pageSize).ToArray();
            if (selected is not null)
            {
                grid.SelectedItem = selected;
            }

            pageLabel.Text = $"{page + 1} / {pages}";
            previous.IsEnabled = page > 0;
            next.IsEnabled = page < pages - 1;
        }

        previous = Button(
            "\u2039",
            () =>
            {
                page--;
                RefreshPage();
            }
        );
        previous.MinWidth = 32;
        previous.ToolTip = "Previous page";
        next = Button(
            "\u203a",
            () =>
            {
                page++;
                RefreshPage();
            }
        );
        next.MinWidth = 32;
        next.ToolTip = "Next page";
        System.Windows.Automation.AutomationProperties.SetName(previous, "Previous page");
        System.Windows.Automation.AutomationProperties.SetName(next, "Next page");
        pager.Children.Add(previous);
        pager.Children.Add(pageLabel);
        pager.Children.Add(next);
        hotkeys.CollectionChanged += (_, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                page = (hotkeys.Count - 1) / pageSize;
            }

            RefreshPage();
        };
        RefreshPage();
        grid.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Delete && grid.SelectedItem is HotkeySettings selected)
            {
                hotkeys.Remove(selected);
                e.Handled = true;
            }
        };
        grid.Columns.Add(
            new DataGridTextColumn
            {
                Header = "Action",
                Binding = new Binding(nameof(HotkeySettings.Function)),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            }
        );
        grid.Columns.Add(
            new DataGridTextColumn
            {
                Header = "Shortcut",
                Binding = new Binding { Converter = new ShortcutConverter() },
                Width = 170,
            }
        );
        grid.Columns.Add(
            new DataGridCheckBoxColumn
            {
                Header = "OSD",
                Binding = new Binding(nameof(HotkeySettings.ShowOsd)),
                Width = 50,
            }
        );
        buttons.Children.Add(Button("Add", () => EditHotkey(true, grid)));
        buttons.Children.Add(Button("Edit", () => EditHotkey(false, grid)));
        buttons.Children.Add(
            Button(
                "Delete",
                () =>
                {
                    if (grid.SelectedItem is HotkeySettings selected)
                    {
                        hotkeys.Remove(selected);
                    }
                }
            )
        );
        grid.MouseDoubleClick += (_, _) => EditHotkey(false, grid);
        panel.Children.Add(grid);
        return panel;
    }
}
