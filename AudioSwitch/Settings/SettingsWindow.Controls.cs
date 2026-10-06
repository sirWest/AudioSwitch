using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private TabItem Tab(string name, UIElement content)
    {
        var item = new TabItem
        {
            Header = name,
            Content = new Border { Child = content, Margin = new Thickness(12) },
        };
        tabs.Items.Add(item);
        return item;
    }

    private static Button Button(string label, Action action)
    {
        var button = new Button
        {
            Content = label,
            MinWidth = 78,
            Margin = new Thickness(3),
            Padding = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static Button Button(string label, Func<bool> action) =>
        Button(
            label,
            () =>
            {
                action();
            }
        );

    private static CheckBox Check(string label, object source, string path)
    {
        var check = new CheckBox
        {
            Content = label,
            MinHeight = 24,
            Padding = new Thickness(8, 0, 0, 0),
            Margin = new Thickness(0, 2, 0, 2),
        };
        check.SetBinding(
            CheckBox.IsCheckedProperty,
            new Binding(path)
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            }
        );
        return check;
    }

    private static void Label(Panel panel, string label) =>
        panel.Children.Add(
            new TextBlock
            {
                Text = label,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4),
            }
        );

    private static ComboBox Choices<T>(
        Panel panel,
        string label,
        object source,
        string path,
        IEnumerable<T> choices
    )
    {
        Label(panel, label);
        var combo = new ComboBox
        {
            ItemsSource = choices,
            MinWidth = 180,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 5),
        };
        combo.SetBinding(
            ComboBox.SelectedItemProperty,
            new Binding(path) { Source = source, Mode = BindingMode.TwoWay }
        );
        panel.Children.Add(combo);
        return combo;
    }

    private static Slider Slider(
        Panel panel,
        string label,
        object source,
        string path,
        double min,
        double max,
        double tick = 1
    )
    {
        var row = new Grid { Height = 34, Margin = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new() { Width = new GridLength(88) });
        row.ColumnDefinitions.Add(new());
        row.Children.Add(
            new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center }
        );
        var slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            TickFrequency = tick,
            IsSnapToTickEnabled = true,
            AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft,
            AutoToolTipPrecision = tick < 1 ? 2 : 0,
            VerticalAlignment = VerticalAlignment.Center,
        };
        slider.SetBinding(
            System.Windows.Controls.Primitives.RangeBase.ValueProperty,
            new Binding(path) { Source = source, Mode = BindingMode.TwoWay }
        );
        Grid.SetColumn(slider, 1);
        row.Children.Add(slider);
        panel.Children.Add(row);
        return slider;
    }
}
