using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed partial class TrayImageTests
{
    [Fact]
    public void ImportNormalizesCommonFormatsAndDeduplicatesWithoutCreatingStorage() =>
        OnSta(() =>
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "AudioSwitch.Tests",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(directory);
            try
            {
                var source = BitmapSource.Create(
                    40,
                    20,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null,
                    Enumerable
                        .Range(0, 40 * 20)
                        .SelectMany(_ => new byte[] { 30, 80, 200, 255 })
                        .ToArray(),
                    160
                );
                foreach (var extension in new[] { "png", "jpg", "bmp", "gif", "tiff", "ico" })
                {
                    var path = Path.Combine(directory, "sample." + extension);
                    {
                        BitmapEncoder encoder = extension switch
                        {
                            "jpg" => new JpegBitmapEncoder(),
                            "bmp" => new BmpBitmapEncoder(),
                            "gif" => new GifBitmapEncoder(),
                            "tiff" => new TiffBitmapEncoder(),
                            _ => new PngBitmapEncoder(),
                        };
                        encoder.Frames.Add(BitmapFrame.Create(source));
                        using var payload = new MemoryStream();
                        encoder.Save(payload);
                        using var file = File.Create(path);
                        if (extension == "ico")
                        {
                            using var writer = new BinaryWriter(
                                file,
                                System.Text.Encoding.UTF8,
                                true
                            );
                            writer.Write((ushort)0);
                            writer.Write((ushort)1);
                            writer.Write((ushort)1);
                            writer.Write((byte)40);
                            writer.Write((byte)20);
                            writer.Write((byte)0);
                            writer.Write((byte)0);
                            writer.Write((ushort)1);
                            writer.Write((ushort)32);
                            writer.Write((uint)payload.Length);
                            writer.Write((uint)22);
                        }
                        file.Write(payload.ToArray());
                    }
                    var bytes = DeviceIcons.Import(path);
                    Assert.Equal(bytes, DeviceIcons.Import(path));
                    using var stream = new MemoryStream(bytes);
                    var image = BitmapDecoder
                        .Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
                        .Frames[0];
                    Assert.Equal(256, image.PixelWidth);
                    Assert.Equal(256, image.PixelHeight);
                    var pixels = Pixels(image);
                    Assert.Equal(0, pixels[3]);
                    Assert.Equal(255, pixels[(128 * 256 + 128) * 4 + 3]);
                    Assert.False(Directory.Exists(Path.Combine(directory, "DeviceIcons")));
                }
                var corrupt = Path.Combine(directory, "corrupt.png");
                File.WriteAllBytes(corrupt, [1, 2, 3]);
                Assert.ThrowsAny<Exception>(() => DeviceIcons.Import(corrupt));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        });

    [Fact]
    public void DeviceIconsFallbackAndMuteBadgePreserveUpperArtworkAtAllTraySizes() =>
        OnSta(() =>
        {
            var directory = Path.Combine(
                Path.GetTempPath(),
                "AudioSwitch.Tests",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(directory);
            try
            {
                var icons = new DeviceIcons(Path.Combine(directory, "settings.json"));
                var device = new AudioDevice(
                    "test",
                    "Speaker",
                    "Speaker",
                    "",
                    Direction.Playback,
                    true,
                    false,
                    false
                );
                var preference = new DeviceSettings { CustomIcon = new string('a', 64) + ".png" };
                var fallback = Pixels(icons.Get(device, null));
                Assert.Equal(fallback, Pixels(icons.Get(device, preference)));
                Directory.CreateDirectory(icons.Store.DirectoryPath);
                File.WriteAllBytes(icons.Store.Resolve(preference.CustomIcon), [1, 2, 3]);
                Assert.Equal(fallback, Pixels(icons.Get(device, preference)));
                var sheet = new DrawingVisual();
                using (var context = sheet.RenderOpen())
                {
                    context.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, 400, 180));
                    var x = 12;
                    foreach (var size in new[] { 16, 20, 24, 32, 48, 64 })
                    {
                        var source = icons.Get(device, null);
                        var normal = Images.DeviceTray(source, false, size);
                        var muted = Images.DeviceTray(source, true, size);
                        var first = Pixels(normal);
                        var second = Pixels(muted);
                        Assert.Equal(
                            first[..(size * (size / 2 - 1) * 4)],
                            second[..(size * (size / 2 - 1) * 4)]
                        );
                        Assert.NotEqual(first, second);
                        var native = Images.Icon(muted, size);
                        Assert.NotEqual(0, native);
                        Assert.True(DestroyIcon(native));
                        context.DrawImage(normal, new Rect(x, 12, size, size));
                        context.DrawImage(muted, new Rect(x, 95, size, size));
                        x += size + 16;
                    }
                }
                var rendered = new RenderTargetBitmap(400, 180, 96, 96, PixelFormats.Pbgra32);
                rendered.Render(sheet);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rendered));
                using var file = File.Create(
                    Path.Combine(AppContext.BaseDirectory, "device-icon-review.png")
                );
                encoder.Save(file);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        });

    [Fact]
    public void DeviceTraySelectionAndTooltipIncludeVolumeForEveryMode()
    {
        Assert.False(DeviceIcons.UsesDeviceIcon(null));
        Assert.False(DeviceIcons.UsesDeviceIcon(new()));
        Assert.True(DeviceIcons.UsesDeviceIcon(new() { UseDeviceIconInTray = true }));
        Assert.False(
            DeviceIcons.UsesDeviceIcon(new() { CustomIcon = new string('a', 64) + ".png" })
        );
        Assert.True(
            DeviceIcons.UsesDeviceIcon(
                new() { CustomIcon = new string('a', 64) + ".png", UseDeviceIconInTray = true }
            )
        );
        Assert.Equal("My headphones\n42%", TrayIcon.Tooltip("My headphones", .42f, false));
        Assert.Equal("Microphone\n0% - Muted", TrayIcon.Tooltip("Microphone", 0, true));
        var longTip = TrayIcon.Tooltip(new string('x', 200), 1, true);
        Assert.Equal(127, longTip.Length);
        Assert.EndsWith("\n100% - Muted", longTip);
    }
}
