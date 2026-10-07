using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using AudioSwitch.Core.Audio;
using AudioSwitch.Shell;

namespace AudioSwitch.Flyout;

internal sealed partial class FlyoutWindow : Window, IDisposable
{
    private readonly App app;

    private Native.FlyoutAnimation? openingAnimation;
    private nint powerNotification;
    private bool dismissing;
    private Direction direction;
    private Direction? directionOverride;

    internal FlyoutWindow(App app)
    {
        this.app = app;
        recoveryTimer.Tick += (_, _) => app.RunSafely(Refresh);
        Title = "AudioSwitch";
        Width = 280;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;
        Topmost = true;
        UseLayoutRounding = true;

        // Acrylic requires a normal HWND, so AllowsTransparency must remain false.
        Resources.MergedDictionaries.Add(
            new ResourceDictionary
            {
                Source = new Uri(
                    "/AudioSwitch;component/Flyout/FlyoutStyles.xaml",
                    UriKind.Relative
                ),
            }
        );

        deviceList.ItemContainerStyle = (Style)FindResource("CompactDeviceItem");
        deviceList.Padding = new Thickness(0);
        ScrollViewer.SetHorizontalScrollBarVisibility(deviceList, ScrollBarVisibility.Disabled);
        volumeSlider.Style = (Style)FindResource("AudioVolumeSlider");
        SetResourceReference(BackgroundProperty, "SolidBackgroundFillColorBaseBrush");

        InitializeBackdrop();
        BuildLayout();
        SubscribeInputEvents();
        SubscribeWindowEvents();
        meterTimer.Tick += (_, _) => UpdateMeters();
        direction = app.Settings.DefaultDirection;
    }

    private void InitializeBackdrop()
    {
        SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");

        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd)?.AddHook(BackdropMessage);
            var powerSavingStatus = new Guid("E00958C0-C213-4ACE-AC77-FECCED2EEEA5");
            powerNotification = Native.RegisterPowerSettingNotification(
                hwnd,
                ref powerSavingStatus,
                0
            );
            ApplyWindowsBackdrop();
        };

        ContentRendered += (_, _) =>
        {
            if (!IsVisible)
            {
                return;
            }

            // Repeat the visual part of Refresh after WPF has presented the surface.
            // The initial positioning happens before that surface has rendered.
            if (!app.Settings.AlwaysVisible && openingAnimation is null)
            {
                app.PositionFlyout(this);
            }

            ApplyWindowsBackdrop();
        };
    }

    private void SubscribeWindowEvents()
    {
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && !app.Settings.AlwaysVisible)
            {
                Dismiss();
            }
        };

        Deactivated += (_, _) =>
        {
            if (!app.Settings.AlwaysVisible)
            {
                app.FlyoutDeactivated();
                Dismiss();
            }
        };

        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible)
            {
                meterTimer.Start();
            }
            else
            {
                dismissing = false;
                openingAnimation?.Dispose();
                openingAnimation = null;
                meterTimer.Stop();

                if (directionOverride is not null)
                {
                    directionOverride = null;

                    app.RunSafely(Refresh);
                }
            }
        };
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ForegroundProperty && IsVisible)
        {
            // Fluent replaces the foreground resource on app/system theme changes.
            // Reapply Acrylic after its own native window-theme update has finished.
            Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() =>
                {
                    if (IsVisible)
                    {
                        ApplyWindowsBackdrop();
                    }
                })
            );
        }
    }

    private void ApplyWindowsBackdrop()
    {
        // Use the resolved Fluent palette, including System and settings previews.
        var surfaceColor = TryFindResource("SolidBackgroundFillColorBase") is Color color
            ? color
            : SystemColors.WindowColor;
        var dark = surfaceColor.R * .2126 + surfaceColor.G * .7152 + surfaceColor.B * .0722 < 128;
        Native.ApplyWindowsBackdrop(this, dark);
    }

    private nint BackdropMessage(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        // Settings, display, power, theme, composition and DWM colorization changes.
        if (message is 0x001A or 0x007E or 0x0218 or 0x031A or 0x031E or 0x0320)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ApplyWindowsBackdrop));
        }

        return 0;
    }

    internal void Open(Direction selected)
    {
        OpenSelected(selected);
    }

    internal void Open() => OpenSelected(null);

    private void OpenSelected(Direction? selected)
    {
        var animate =
            (!IsVisible || dismissing)
            && !app.Settings.AlwaysVisible
            && SystemParameters.ClientAreaAnimation;
        openingAnimation?.Dispose();
        openingAnimation = null;
        dismissing = false;
        Native.WithoutPresentation(
            this,
            () =>
            {
                directionOverride = selected;

                Refresh();
                ShowActivated = true;

                Show();

                ApplyWindowsBackdrop();

                if (
                    app.Settings.AlwaysVisible
                    && app.Settings.FlyoutLeft is double x
                    && app.Settings.FlyoutTop is double y
                )
                {
                    Native.ClampToMonitor(this, (int)x, (int)y);
                }
                else
                {
                    app.PositionFlyout(this);
                }

                UpdateLayout();
                if (animate)
                {
                    openingAnimation = app.PrepareFlyoutAnimation(this);
                }
            }
        );

        var hwnd = new WindowInteropHelper(this).Handle;

        if (hwnd != IntPtr.Zero)
        {
            Native.SetForegroundWindow(hwnd);
        }

        Activate();

        deviceList.Focus();
        Keyboard.Focus(deviceList);
        openingAnimation?.Start();
    }

    internal void Dismiss()
    {
        if (!IsVisible || dismissing)
        {
            return;
        }

        openingAnimation?.Dispose();
        openingAnimation = null;
        if (app.Settings.AlwaysVisible || !SystemParameters.ClientAreaAnimation)
        {
            Hide();
            return;
        }
        dismissing = true;
        openingAnimation = app.PrepareFlyoutDismissal(this, Hide);
        openingAnimation.Start();
    }

    internal void RefreshAfterFailure()
    {
        deviceList.SelectedItem = null;
        Refresh();
    }

    internal void Refresh()
    {
        audioReadFailed = false;
        if (dismissing)
        {
            Hide();
        }

        openingAnimation?.Dispose();
        openingAnimation = null;
        var both = directionOverride is null && app.Settings.ShowBothDeviceGroups;
        direction =
            directionOverride ?? (both ? Direction.Playback : app.Settings.DefaultDirection);
        var devices = ReadDevices(direction);
        defaultDevice = devices.FirstOrDefault(d => d.Multimedia);

        header.Visibility = both ? Visibility.Collapsed : Visibility.Visible;
        (settingsButton.Parent as Panel)?.Children.Remove(settingsButton);
        if (!both)
        {
            DockPanel.SetDock(settingsButton, Dock.Right);
            header.Children.Insert(0, settingsButton);
        }

        title.Text = direction == Direction.Playback ? "Playback" : "Recording";

        leftMeter.Colorful = rightMeter.Colorful = app.Settings.ColorVu;

        var selectedId = (deviceList.SelectedItem as ListBoxItem)?.Tag is AudioDevice selected
            ? selected.Id
            : null;

        deviceList.Items.Clear();

        if (both)
        {
            AddGroupHeading("Playback");
            AddDevices(devices, selectedId, "No visible playback devices");
            AddGroupHeading("Recording");
            AddDevices(
                ReadDevices(Direction.Recording),
                selectedId,
                "No visible recording devices"
            );
        }
        else
        {
            AddDevices(devices, selectedId, "No visible audio devices");
        }

        RefreshMonitors();

        if (IsVisible && !app.Settings.AlwaysVisible)
        {
            UpdateLayout();

            app.PositionFlyout(this);
        }
    }

    public void Dispose()
    {
        // WPF may have already destroyed the window before App.OnExit runs.
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != 0)
        {
            HwndSource.FromHwnd(hwnd)?.RemoveHook(BackdropMessage);
        }

        if (powerNotification != 0)
        {
            Native.UnregisterPowerSettingNotification(powerNotification);
            powerNotification = 0;
        }

        openingAnimation?.Dispose();
        openingAnimation = null;
        var previousTray = trayMonitor;
        trayMonitor = null;
        previousTray?.Dispose();
        meterTimer.Stop();
        recoveryTimer.Stop();
        var previous = monitor;
        monitor = null;
        previous?.Dispose();
    }
}
