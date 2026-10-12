using System.IO;
using System.Windows.Media.Imaging;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch.Imaging;

internal sealed class DeviceIcons(string settingsPath)
{
    internal readonly DeviceIconStore Store = new(settingsPath);
    private readonly Dictionary<string, BitmapSource> cache = new();

    internal static byte[] Import(string path)
    {
        if (new FileInfo(path).Length > 32 * 1024 * 1024)
            throw new InvalidDataException("Choose an image smaller than 32 MB.");
        using var input = File.OpenRead(path);
        var decoder = BitmapDecoder.Create(
            input,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad
        );
        var source =
            decoder is IconBitmapDecoder
                ? decoder
                    .Frames.OrderByDescending(f => f.PixelWidth)
                    .ThenByDescending(f => f.Format.BitsPerPixel)
                    .First()
                : decoder.Frames[0];
        ValidateSize(source.PixelWidth, source.PixelHeight);
        var normalized = Images.DeviceTray(source, false, 256);
        using var output = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(normalized));
        encoder.Save(output);
        return output.ToArray();
    }

    private static void ValidateSize(int width, int height)
    {
        if (width <= 0 || height <= 0 || (long)width * height > 40_000_000)
            throw new InvalidDataException("Choose an image with at most 40 million pixels.");
    }

    internal BitmapSource Get(
        AudioDevice device,
        DeviceSettings? preference,
        IReadOnlyDictionary<string, byte[]>? pending = null
    )
    {
        var name = preference?.CustomIcon;
        if (!string.IsNullOrEmpty(name) && DeviceIconStore.IsValidName(name))
        {
            if (pending is not null && pending.TryGetValue(name, out var bytes))
            {
                using var stream = new MemoryStream(bytes);
                var image = BitmapDecoder
                    .Create(
                        stream,
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.OnLoad
                    )
                    .Frames[0];
                image.Freeze();
                return image;
            }
            try
            {
                var path = Store.Resolve(name);
                if (File.Exists(path))
                {
                    if (!cache.TryGetValue(name, out var image))
                        cache[name] = image = Images.Load(path);
                    return image;
                }
            }
            catch (Exception ex)
                when (ex
                        is IOException
                            or UnauthorizedAccessException
                            or NotSupportedException
                            or ArgumentException
                            or System.Runtime.InteropServices.ExternalException
                )
            {
                System.Diagnostics.Trace.TraceWarning($"Device icon unavailable: {ex.Message}");
            }
        }
        return (BitmapSource)Native.DeviceIcon(device.IconPath);
    }

    internal void ClearCache() => cache.Clear();

    internal static bool UsesDeviceIcon(DeviceSettings? preference) =>
        preference?.UseDeviceIconInTray == true;
}
