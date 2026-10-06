using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AudioSwitch.Core.Audio;
using AudioSwitch.Shell;

namespace AudioSwitch.Flyout;

internal sealed partial class FlyoutWindow
{
    private readonly DockPanel header = new() { Margin = new Thickness(0, 0, 0, 3) };

    private readonly Button settingsButton = new()
    {
        Content = "\u2699",
        ToolTip = "Settings",
        Width = 26,
        Height = 24,
        Padding = new Thickness(0),
        Background = Brushes.Transparent,
        BorderThickness = new Thickness(0),
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
    };

    private readonly ListBox deviceList = new()
    {
        BorderThickness = new Thickness(0),
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
        MaxHeight = 430,
        FocusVisualStyle = null,
        Background = Brushes.Transparent,
    };

    private readonly Slider volumeSlider = new()
    {
        Minimum = 0,
        Maximum = 1,
        SmallChange = .02,
        LargeChange = .1,
    };

    private readonly VuMeter leftMeter = new() { Height = 4 };

    private readonly VuMeter rightMeter = new() { Height = 4 };

    private readonly TextBlock title = new() { Margin = new Thickness(5), FontSize = 12 };

    private void BuildLayout()
    {
        var root = new Border { Background = Brushes.Transparent };
        Content = root;

        var layout = new DockPanel { Background = Brushes.Transparent };
        root.Child = layout;

        var bottomSurface = new Border { Padding = new Thickness(18, 12, 18, 14) };
        bottomSurface.SetResourceReference(Border.BackgroundProperty, "FlyoutBottomSurfaceBrush");
        DockPanel.SetDock(bottomSurface, Dock.Bottom);
        layout.Children.Add(bottomSurface);

        var controls = new Grid();
        controls.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
        controls.RowDefinitions.Add(new RowDefinition { Height = new GridLength(16) });
        controls.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
        Grid.SetRow(volumeSlider, 1);
        Grid.SetRow(rightMeter, 2);
        controls.Children.Add(leftMeter);
        controls.Children.Add(volumeSlider);
        controls.Children.Add(rightMeter);
        bottomSurface.Child = controls;

        var mainSurface = new Border { Padding = new Thickness(8) };
        mainSurface.SetResourceReference(Border.BackgroundProperty, "FlyoutTopSurfaceBrush");
        layout.Children.Add(mainSurface);

        var panel = new Grid { Background = Brushes.Transparent };
        mainSurface.Child = panel;
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        panel.Children.Add(header);
        header.Children.Add(title);
        Grid.SetRow(deviceList, 1);
        panel.Children.Add(deviceList);
    }

    private void AddDevices(
        IEnumerable<AudioDevice> devices,
        string? selectedId,
        string emptyMessage
    )
    {
        var visibleDevices = devices
            .Where(device => app.Settings.ForDevice(device.Id)?.Hidden != true)
            .ToArray();
        foreach (var device in visibleDevices)
        {
            AddDeviceItem(device, selectedId);
        }

        if (visibleDevices.Length == 0)
        {
            deviceList.Items.Add(new ListBoxItem { Content = emptyMessage, IsEnabled = false });
        }
    }

    private void AddDeviceItem(AudioDevice device, string? selectedId)
    {
        var row = new Grid { MinHeight = 32 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
        row.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );

        var imagePanel = new Grid { Width = 32, Height = 32 };
        imagePanel.Children.Add(
            new Image
            {
                Source = Native.DeviceIcon(device.IconPath),
                Width = 32,
                Height = 32,
            }
        );

        if (device.Multimedia)
        {
            imagePanel.Children.Add(CreateDefaultDeviceBadge());
        }

        row.Children.Add(imagePanel);

        var text = new TextBlock
        {
            Text = device.DisplayName(app.Settings),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(3, 0, 0, 0),
        };

        Grid.SetColumn(text, 1);
        row.Children.Add(text);

        var item = new ListBoxItem
        {
            Content = row,
            Tag = device,
            IsSelected = device.Id == (selectedId ?? defaultDevice?.Id),
            ToolTip = device.Name,
        };

        deviceList.Items.Add(item);
    }

    private static FrameworkElement CreateDefaultDeviceBadge()
    {
        var container = new Grid
        {
            Width = 32,
            Height = 32,
            IsHitTestVisible = false,
        };

        var badge = new Grid
        {
            Width = 15,
            Height = 15,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, -1, -1),
            SnapsToDevicePixels = false,
            UseLayoutRounding = false,
        };

        // Resolve the accent dynamically so a Windows color change updates the badge.
        var background = new Ellipse { Width = 15, Height = 15 };
        background.SetResourceReference(Shape.FillProperty, SystemColors.AccentColorLight2BrushKey);
        badge.Children.Add(background);

        // The outline keeps the badge readable on light device icons.
        var border = new Ellipse
        {
            Width = 15,
            Height = 15,
            Fill = Brushes.Transparent,
            Stroke = new SolidColorBrush(Color.FromArgb(75, 0, 0, 0)),
            StrokeThickness = 1,
        };

        badge.Children.Add(border);

        var check = new Path
        {
            Data = Geometry.Parse("M 0,3.6 L 3.2,6.7 L 9.2,0"),
            Width = 9.2,
            Height = 6.7,
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Stroke = Brushes.White,
            StrokeThickness = 1.9,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            SnapsToDevicePixels = false,
        };

        badge.Children.Add(check);
        container.Children.Add(badge);

        return container;
    }

    private void AddGroupHeading(string name)
    {
        var heading = new DockPanel();
        var includeSettings = deviceList.Items.Count == 0;
        if (includeSettings)
        {
            DockPanel.SetDock(settingsButton, Dock.Right);
            heading.Children.Add(settingsButton);
        }

        heading.Children.Add(
            new TextBlock
            {
                Text = name,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(3, 6, 0, 3),
                IsHitTestVisible = false,
            }
        );
        deviceList.Items.Add(
            new ListBoxItem
            {
                Content = heading,
                Style = (Style)FindResource("DeviceGroupHeading"),
                Focusable = false,
                IsHitTestVisible = includeSettings,
            }
        );
    }
}
