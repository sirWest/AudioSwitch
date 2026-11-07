using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Settings;

internal sealed partial class ScrollModifierPicker : Button
{
    private static readonly (ScrollKeys Key, string ShortName)[] Options =
    [
        (ScrollKeys.LeftMouseButton, "Left mouse"),
        (ScrollKeys.RightMouseButton, "Right mouse"),
        (ScrollKeys.Control, "Ctrl"),
        (ScrollKeys.Alt, "Alt"),
        (ScrollKeys.LWin, "Left Win"),
        (ScrollKeys.RWin, "Right Win"),
        (ScrollKeys.Shift, "Shift"),
    ];

    private AppSettings? settings;

    public ScrollModifierPicker()
    {
        InitializeComponent();
        SetResourceReference(StyleProperty, typeof(Button));
    }

    internal void Initialize(AppSettings settings)
    {
        this.settings = settings;
        foreach (MenuItem item in menu.Items)
            item.IsChecked = settings.ScrollKeys.HasFlag((ScrollKeys)item.Tag);
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (settings is null)
            return;
        var text = string.Join(
            " + ",
            Options
                .Where(option => settings.ScrollKeys.HasFlag(option.Key))
                .Select(option => option.ShortName)
        );
        summary.Text = text.Length == 0 ? "Select modifiers" : text;
        ToolTip = summary.Text;
    }

    private void ModifierChanged(object sender, RoutedEventArgs e)
    {
        if (settings is null || sender is not MenuItem item)
            return;
        var key = (ScrollKeys)item.Tag;
        if (item.IsChecked)
            settings.ScrollKeys |= key;
        else
            settings.ScrollKeys &= ~key;
        UpdateSummary();
    }

    private void AnimateArrow(double angle) =>
        arrowRotation.BeginAnimation(
            RotateTransform.AngleProperty,
            new DoubleAnimation
            {
                To = angle,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            }
        );

    private void MenuOpened(object sender, RoutedEventArgs e) => AnimateArrow(180);

    private void MenuClosed(object sender, RoutedEventArgs e) => AnimateArrow(0);

    private void ToggleMenu(object sender, RoutedEventArgs e)
    {
        menu.PlacementTarget = this;
        menu.IsOpen = !menu.IsOpen;
    }

    private void OpenMenuFromKeyboard(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key is not (System.Windows.Input.Key.Down or System.Windows.Input.Key.F4))
            return;
        menu.PlacementTarget = this;
        menu.IsOpen = true;
        ((MenuItem)menu.Items[0]).Focus();
        e.Handled = true;
    }
}
