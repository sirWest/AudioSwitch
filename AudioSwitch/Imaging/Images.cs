using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Imaging;

internal static class Images
{
    private static readonly Dictionary<string, BitmapSource> trayAssets = new();

    internal static string VolumeAsset(float volume, bool muted) =>
        muted ? "mute.png"
        : volume <= 0 ? "0.png"
        : volume <= .25f ? "0-25.png"
        : volume <= .5f ? "25-50.png"
        : volume <= .75f ? "50-75.png"
        : "75-100.png";

    internal static BitmapSource TrayAsset(string name)
    {
        if (trayAssets.TryGetValue(name, out var cached))
        {
            return cached;
        }

        var source = Asset(name == "0.png" ? "0-25.png" : name);
        var bounds = new Rect(0, 0, source.PixelWidth, source.PixelHeight);
        var radius = Math.Min(bounds.Width, bounds.Height) * .1875;
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            // Round the background without adding padding to the shell's icon slot.
            context.PushClip(new RectangleGeometry(bounds, radius, radius));
            context.DrawImage(source, bounds);
            if (name == "0.png")
            {
                // The speaker occupies the left half; cover all sound marks with the asset's background.
                var background = new byte[4];
                new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0).CopyPixels(
                    new Int32Rect(0, 0, 1, 1),
                    background,
                    4,
                    0
                );
                var brush = new SolidColorBrush(
                    Color.FromArgb(background[3], background[2], background[1], background[0])
                );
                context.DrawRectangle(
                    brush,
                    null,
                    new Rect(bounds.Width / 2, 0, bounds.Width / 2, bounds.Height)
                );
            }
            context.Pop();
        }

        var result = new RenderTargetBitmap(
            source.PixelWidth,
            source.PixelHeight,
            96,
            96,
            PixelFormats.Pbgra32
        );
        result.Render(visual);
        result.Freeze();
        trayAssets.Add(name, result);
        return result;
    }

    internal static BitmapSource Resize(BitmapSource source, int size)
    {
        var result = new TransformedBitmap(
            source,
            new ScaleTransform((double)size / source.PixelWidth, (double)size / source.PixelHeight)
        );
        result.Freeze();
        return result;
    }

    internal static BitmapSource Load(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(Path.GetFullPath(path));
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }

    internal static BitmapSource Asset(string name)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri($"pack://application:,,,/AudioSwitch;component/Assets/{name}");
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }

    internal static BitmapSource Tint(BitmapSource source, DeviceSettings? settings)
    {
        if (
            settings is null
            || settings.Hue == 0 && settings.Saturation == 0 && settings.Brightness == 0
        )
        {
            return source;
        }

        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var stride = converted.PixelWidth * 4;
        var bytes = new byte[stride * converted.PixelHeight];
        converted.CopyPixels(bytes, stride, 0);
        // Replace hue while retaining the source's lightness and saturation, then apply
        // the user's offsets. BGRA's fourth byte is alpha and must remain unchanged.
        for (var i = 0; i < bytes.Length; i += 4)
        {
            var r = bytes[i + 2] / 255d;
            var g = bytes[i + 1] / 255d;
            var b = bytes[i] / 255d;
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var light = (max + min) / 2;
            var saturation = max == min ? 0 : (max - min) / (1 - Math.Abs(2 * light - 1));
            light = Math.Clamp(light + settings.Brightness / 100d, 0, 1);
            saturation = Math.Clamp(saturation + settings.Saturation / 100d, 0, 1);
            var chroma = (1 - Math.Abs(2 * light - 1)) * saturation;
            var hue = ((settings.Hue % 360 + 360) % 360) / 60d;
            var x = chroma * (1 - Math.Abs(hue % 2 - 1));
            var m = light - chroma / 2;
            (r, g, b) = (int)hue switch
            {
                0 => (chroma, x, 0d),
                1 => (x, chroma, 0d),
                2 => (0d, chroma, x),
                3 => (0d, x, chroma),
                4 => (x, 0d, chroma),
                _ => (chroma, 0d, x),
            };
            bytes[i] = (byte)Math.Round((b + m) * 255);
            bytes[i + 1] = (byte)Math.Round((g + m) * 255);
            bytes[i + 2] = (byte)Math.Round((r + m) * 255);
        }

        var result = BitmapSource.Create(
            converted.PixelWidth,
            converted.PixelHeight,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            bytes,
            stride
        );
        result.Freeze();
        return result;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateIconFromResourceEx(
        byte[] data,
        uint size,
        bool icon,
        uint version,
        int width,
        int height,
        uint flags
    );

    internal static nint Icon(BitmapSource image, int size = 32)
    {
        using var stream = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(Resize(image, size)));
        encoder.Save(stream);
        var bytes = stream.ToArray();
        var icon = CreateIconFromResourceEx(
            bytes,
            (uint)bytes.Length,
            true,
            0x30000,
            size,
            size,
            0
        );
        if (icon == 0)
        {
            throw new System.ComponentModel.Win32Exception(
                Marshal.GetLastWin32Error(),
                "Could not create the tray icon."
            );
        }

        return icon;
    }
}
