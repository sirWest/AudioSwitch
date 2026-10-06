using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using AudioSwitch.Shell;

namespace AudioSwitch.Osd;

internal sealed partial class OsdWindow : Window
{
    private readonly Canvas canvas = new();
    private readonly DispatcherTimer timer = new();
    private OsdSettings settings = new();
    private XElement? skin;
    private BitmapSource? background,
        mute,
        meter,
        effect,
        deviceBackground;
    private string? loadedSkin;
    private bool loadedDark;
    private float lastVolume;
    private bool lastMuted;
    private string? lastDevice;
    private static readonly DependencyProperty ThemeSurfaceProperty = DependencyProperty.Register(
        "ThemeSurface",
        typeof(Color),
        typeof(OsdWindow),
        new PropertyMetadata(Colors.White, (owner, _) => ((OsdWindow)owner).RefreshTheme())
    );
    private bool IsDark => ((Color)GetValue(ThemeSurfaceProperty)).R < 128;
    private bool positioning;
    internal bool Preview { get; private set; }

    internal event Action<int, int>? Moved;
    internal string Author => (string?)skin?.Element("Author") ?? "";
    internal string Website => (string?)skin?.Element("Website") ?? "";
    internal string SkinVersion => (string?)skin?.Attribute("Version") ?? "";
    internal static string SkinRoot => Path.Combine(AppContext.BaseDirectory, "Skins");
    internal static string[] Skins =>
        Directory.Exists(SkinRoot)
            ? Directory
                .GetDirectories(SkinRoot)
                .Where(p => File.Exists(Path.Combine(p, "skin.xml")))
                .Select(Path.GetFileName)
                .OfType<string>()
                .Order()
                .ToArray()
            : [];

    internal OsdWindow()
    {
        Title = "AudioSwitch OSD";
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;
        ResizeMode = ResizeMode.NoResize;
        Content = canvas;
        SetResourceReference(ThemeSurfaceProperty, "SolidBackgroundFillColorBase");
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            Native.SetWindowLongPtr(
                hwnd,
                -20,
                Native.GetWindowLongPtr(hwnd, -20) | 0x08000000 | 0x80
            );
        };
        MouseLeftButtonDown += (_, e) =>
        {
            if (!Preview)
            {
                return;
            }

            DragMove();
            Native.GetWindowRect(new WindowInteropHelper(this).Handle, out var rect);
            Moved?.Invoke(rect.Left, rect.Top);
            e.Handled = true;
        };
        timer.Tick += (_, _) => Fade();
    }

    private void Load(OsdSettings value)
    {
        settings = value;
        if (loadedSkin == value.Skin && loadedDark == IsDark)
        {
            return;
        }

        if (Path.GetFileName(value.Skin) != value.Skin)
        {
            throw new InvalidDataException("Invalid skin name.");
        }

        var directory = Path.Combine(SkinRoot, value.Skin);
        var document = XDocument.Load(Path.Combine(directory, "skin.xml"));
        var root = document.Root ?? throw new InvalidDataException("Empty skin.");
        var bar = root.Element("VolBar") ?? throw new InvalidDataException("Missing volume bar.");
        if ((int?)bar.Attribute("Steps") is not > 0)
        {
            throw new InvalidDataException("Invalid skin steps.");
        }

        var dark = IsDark;
        background = Images.Load(
            OsdSkin.ImagePath(directory, root, dark, "Background", "back.png")
        );
        mute = Images.Load(OsdSkin.ImagePath(directory, root, dark, "Mute", "mute.png"));
        meter = Images.Load(OsdSkin.ImagePath(directory, root, dark, "Meter", "meter.png"));
        effect =
            (bool?)bar.Attribute("Effect") == true
                ? Images.Load(
                    OsdSkin.ImagePath(directory, root, dark, "Effect", "meter_effect.png")
                )
                : null;
        var images = root.Element("Images");
        deviceBackground =
            images?.Attribute("DeviceBackground") is not null
            || images?.Element(dark ? "Dark" : "Light")?.Attribute("DeviceBackground") is not null
                ? Images.Load(
                    OsdSkin.ImagePath(directory, root, dark, "DeviceBackground", "back.png")
                )
                : null;
        skin = root;
        Width = canvas.Width = background.PixelWidth;
        Height = canvas.Height = background.PixelHeight;
        loadedSkin = value.Skin;
        loadedDark = dark;
    }

    private void RefreshTheme()
    {
        if (loadedSkin is null || !IsVisible)
        {
            return;
        }

        Load(settings);
        Render(lastVolume, lastMuted, lastDevice);
    }

    internal void Display(
        OsdSettings value,
        float volume = .75f,
        bool muted = false,
        string? device = null,
        bool preview = false
    )
    {
        positioning = true;
        try
        {
            Load(value);
            Preview = preview;
            timer.Stop();
            BeginAnimation(OpacityProperty, null);
            Opacity = value.Opacity;
            Render(volume, muted, device);
            Show();
            var hwnd = new WindowInteropHelper(this).Handle;
            var style = Native.GetWindowLongPtr(hwnd, -20);
            Native.SetWindowLongPtr(hwnd, -20, preview ? style & ~0x20 : style | 0x20);
            Native.PositionOsd(this, (int)value.Left, (int)value.Top);
            if (!preview)
            {
                timer.Interval = TimeSpan.FromMilliseconds(value.Timeout);
                timer.Start();
            }
        }
        finally
        {
            positioning = false;
        }
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        if (!Preview || positioning || !IsVisible)
        {
            return;
        }

        Native.GetWindowRect(new WindowInteropHelper(this).Handle, out var rect);
        Moved?.Invoke(rect.Left, rect.Top);
    }

    internal void EndPreview()
    {
        Preview = false;
        timer.Stop();
        BeginAnimation(OpacityProperty, null);
        Hide();
    }

    private void Fade()
    {
        timer.Stop();
        var animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180));
        animation.Completed += (_, _) =>
        {
            if (!Preview && !timer.IsEnabled)
            {
                Hide();
            }
        };
        BeginAnimation(OpacityProperty, animation);
    }
}
