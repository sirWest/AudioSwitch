using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioSwitch.Flyout;

internal sealed partial class FlyoutWindow
{
    private Point dragStart;
    private DateTime draggedAt;

    private void TitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (app.Settings.AlwaysVisible)
        {
            DragMove();
            app.RunSafely(() => app.SavePosition(this));
        }
    }

    private void VolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!updatingVolume && monitor is not null)
        {
            app.RunSafely(() => app.Audio.SetVolume(monitor.Id, (float)volumeSlider.Value));
        }
    }

    private void VolumeSliderMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (monitor is not null)
        {
            app.RunSafely(() => app.Audio.SetMute(monitor.Id));
        }

        e.Handled = true;
    }

    private void WindowPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        app.RunSafely(() => app.ChangeVolume(direction, e.Delta, false, true));

        e.Handled = true;
    }

    private void DeviceListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        dragStart = e.GetPosition(this);
    }

    private void DeviceListPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!app.Settings.AlwaysVisible || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var delta = e.GetPosition(this) - dragStart;

        if (
            Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance
        )
        {
            return;
        }

        DragMove();
        draggedAt = DateTime.UtcNow;

        app.RunSafely(() => app.SavePosition(this));

        e.Handled = true;
    }

    private void DeviceListPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (DateTime.UtcNow - draggedAt < TimeSpan.FromMilliseconds(400))
        {
            // DragMove consumes the gesture, but WPF may still deliver its mouse-up.
            return;
        }

        var item = e.OriginalSource is DependencyObject source
            ? ItemsControl.ContainerFromElement(deviceList, source) as ListBoxItem
            : null;

        if (item?.Content is FlyoutRow { Device: { } device })
        {
            app.RunSafely(() => app.Select(device));
        }
    }

    private void DeviceListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && deviceList.SelectedItem is FlyoutRow { Device: { } device })
        {
            app.RunSafely(() => app.Select(device));

            e.Handled = true;
        }
    }
}
