using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Settings;

internal sealed class ScrollModifierPicker : Button
{
    private static readonly (ScrollKeys Key, string Name, string ShortName)[] Options =
    [
        (ScrollKeys.LeftMouseButton, "Left mouse button", "Left mouse"),
        (ScrollKeys.RightMouseButton, "Right mouse button", "Right mouse"),
        (ScrollKeys.Control, "Ctrl", "Ctrl"),
        (ScrollKeys.Alt, "Alt", "Alt"),
        (ScrollKeys.LWin, "Left Windows key", "Left Win"),
        (ScrollKeys.RWin, "Right Windows key", "Right Win"),
        (ScrollKeys.Shift, "Shift", "Shift"),
    ];

    internal ScrollModifierPicker(AppSettings settings)
    {
        SetResourceReference(StyleProperty, typeof(Button));
        Width = 340;
        HorizontalAlignment = HorizontalAlignment.Left;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        Padding = new Thickness(10, 5, 10, 5);
        Margin = new Thickness(0, 4, 0, 6);
        AutomationProperties.SetName(this, "Mouse-wheel modifiers");
        var content = new DockPanel();
        var arrow = new Path
        {
            Data = Geometry.Parse("M 0,0 L 4,4 L 8,0 L 7,0 L 4,3 L 1,0 Z"),
            Width = 8,
            Height = 4,
            Stretch = Stretch.Fill,
            Margin = new Thickness(12, 0, 2, 0),
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = new Point(0.5, 0.5),
        };
        arrow.SetBinding(
            Shape.FillProperty,
            new System.Windows.Data.Binding(nameof(Foreground)) { Source = this }
        );
        var arrowRotation = new RotateTransform(0);
        arrow.RenderTransform = arrowRotation;
        DockPanel.SetDock(arrow, Dock.Right);
        content.Children.Add(arrow);
        var summary = new TextBlock
        {
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        content.Children.Add(summary);
        Content = content;
        var menu = new ContextMenu
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            MinWidth = Width,
        };
        ContextMenu = menu;
        void AnimateArrow(double angle)
        {
            var animation = new DoubleAnimation
            {
                To = angle,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            arrowRotation.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        menu.Opened += (_, _) => AnimateArrow(180);
        menu.Closed += (_, _) => AnimateArrow(0);
        void UpdateSummary()
        {
            var selected = Options
                .Where(option => settings.ScrollKeys.HasFlag(option.Key))
                .Select(option => option.ShortName);
            var text = string.Join(" + ", selected);
            summary.Text = text.Length == 0 ? "Select modifiers" : text;
            ToolTip = summary.Text;
        }

        foreach (var (key, name, _) in Options)
        {
            var item = new MenuItem
            {
                Header = name,
                IsCheckable = true,
                IsChecked = settings.ScrollKeys.HasFlag(key),
                StaysOpenOnClick = true,
            };
            item.Checked += (_, _) =>
            {
                settings.ScrollKeys |= key;
                UpdateSummary();
            };
            item.Unchecked += (_, _) =>
            {
                settings.ScrollKeys &= ~key;
                UpdateSummary();
            };
            menu.Items.Add(item);
        }

        Click += (_, _) =>
        {
            menu.IsOpen = !menu.IsOpen;
        };
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Down || e.Key == System.Windows.Input.Key.F4)
            {
                menu.IsOpen = true;
                if (menu.Items.Count > 0)
                {
                    ((MenuItem)menu.Items[0]).Focus();
                }

                e.Handled = true;
            }
        };
        UpdateSummary();
    }
}
