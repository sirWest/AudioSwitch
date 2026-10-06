using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Flyout;

internal sealed partial class FlyoutWindow
{
    private Point dragStart;
    private DateTime draggedAt;

    private void SubscribeInputEvents()
    {
        settingsButton.Click += (_, _) => app.OpenSettings();
        title.MouseLeftButtonDown += (_, _) =>
        {
            if (app.Settings.AlwaysVisible)
            {
                DragMove();
                app.RunSafely(() => app.SavePosition(this));
            }
        };

        volumeSlider.ValueChanged += (_, _) =>
        {
            if (!updatingVolume && monitor is not null)
            {
                app.RunSafely(() => app.Audio.SetVolume(monitor.Id, (float)volumeSlider.Value));
            }
        };

        volumeSlider.MouseRightButtonUp += (_, e) =>
        {
            if (monitor is not null)
            {
                app.RunSafely(() => app.Audio.SetMute(monitor.Id));
            }

            e.Handled = true;
        };

        PreviewMouseWheel += (_, e) =>
        {
            app.RunSafely(() => app.ChangeVolume(direction, e.Delta, false, true));

            e.Handled = true;
        };

        deviceList.PreviewMouseLeftButtonDown += (_, e) =>
        {
            dragStart = e.GetPosition(this);
        };

        deviceList.PreviewMouseMove += (_, e) =>
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
        };

        deviceList.PreviewMouseLeftButtonUp += (_, e) =>
        {
            if (DateTime.UtcNow - draggedAt < TimeSpan.FromMilliseconds(400))
            {
                // DragMove consumes the gesture, but WPF may still deliver its mouse-up.
                return;
            }

            var item =
                ItemsControl.ContainerFromElement(deviceList, e.OriginalSource as DependencyObject)
                as ListBoxItem;

            if (item?.Tag is AudioDevice device)
            {
                app.RunSafely(() => app.Select(device));
            }
        };

        deviceList.KeyDown += (_, e) =>
        {
            if (
                e.Key == Key.Enter
                && deviceList.SelectedItem is ListBoxItem { Tag: AudioDevice device }
            )
            {
                app.RunSafely(() => app.Select(device));

                e.Handled = true;
            }
        };
    }
}
