using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private UIElement CreateAboutTab()
    {
        const string repository = "https://github.com/sirWest/AudioSwitch";
        var assembly = typeof(App).Assembly;
        var name =
            assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
            ?? assembly.GetName().Name
            ?? "AudioSwitch";
        var version =
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "Unknown";
        var panel = new StackPanel { Margin = new Thickness(12, 16, 12, 12) };
        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 20) };
        var iconFrames = BitmapDecoder.Create(
            new Uri("pack://application:,,,/AudioSwitch;component/Assets/AudioSwitchIcon.ico"),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad
        );
        var icon = new Image
        {
            Source = iconFrames.Frames.MaxBy(frame => frame.PixelWidth),
            Width = 64,
            Height = 64,
            Margin = new Thickness(0, 0, 16, 0),
        };
        RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);
        DockPanel.SetDock(icon, Dock.Left);
        header.Children.Add(icon);
        var identity = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        identity.Children.Add(
            new TextBlock
            {
                Text = name,
                FontSize = 28,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
            }
        );
        identity.Children.Add(AboutText($"Version {version}", secondary: true));
        header.Children.Add(identity);
        panel.Children.Add(header);
        panel.Children.Add(
            AboutText(
                "Switch audio devices, adjust volume, and control your Windows sound with global shortcuts from the system tray."
            )
        );

        var credits = new StackPanel { Margin = new Thickness(0, 20, 0, 20) };
        credits.Children.Add(
            new TextBlock { Text = "Made by Tanel Teede", FontWeight = FontWeights.SemiBold }
        );
        credits.Children.Add(AboutText("Estonia  |  Project started in 2015", secondary: true));
        panel.Children.Add(credits);

        var divider = new Border { Height = 1, Margin = new Thickness(0, 0, 0, 16) };
        divider.SetResourceReference(Border.BackgroundProperty, "DividerStrokeColorDefaultBrush");
        panel.Children.Add(divider);
        panel.Children.Add(AboutLink("GitHub project", repository));
        panel.Children.Add(AboutLink("Releases and changelog", repository + "/releases"));
        panel.Children.Add(
            AboutLink("Report an issue or request a feature", repository + "/issues")
        );
        var license = AboutLink(
            "Open source under the Apache License 2.0",
            repository + "/blob/master/LICENSE"
        );
        license.Margin = new Thickness(0, 16, 0, 0);
        panel.Children.Add(license);

        return new ScrollViewer
        {
            Content = panel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
    }

    private static TextBlock AboutText(string text, bool secondary = false)
    {
        var block = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0),
            FontSize = 14,
        };
        block.SetResourceReference(
            TextBlock.ForegroundProperty,
            secondary ? "TextFillColorSecondaryBrush" : "TextFillColorPrimaryBrush"
        );
        return block;
    }

    private TextBlock AboutLink(string label, string address)
    {
        var link = new Hyperlink(new Run(label))
        {
            NavigateUri = new Uri(address),
            ToolTip = address,
        };
        link.SetResourceReference(
            TextElement.ForegroundProperty,
            "AccentTextFillColorPrimaryBrush"
        );
        link.RequestNavigate += (_, e) =>
        {
            e.Handled = true;
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                statusText.Text = $"Could not open the link: {ex.Message}";
            }
        };
        var block = new TextBlock
        {
            Margin = new Thickness(0, 5, 0, 5),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
        };
        block.Inlines.Add(link);
        return block;
    }
}
