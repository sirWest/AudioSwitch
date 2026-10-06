using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class TrayImageTests
{
    private static readonly string[] Assets =
    [
        "0.png",
        "0-25.png",
        "25-50.png",
        "50-75.png",
        "75-100.png",
        "mute.png",
    ];

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(nint icon);

    [Theory]
    [InlineData(0, "0.png")]
    [InlineData(.00001f, "0-25.png")]
    [InlineData(.25f, "0-25.png")]
    [InlineData(.25001f, "25-50.png")]
    [InlineData(.5f, "25-50.png")]
    [InlineData(.50001f, "50-75.png")]
    [InlineData(.75f, "50-75.png")]
    [InlineData(.75001f, "75-100.png")]
    [InlineData(1, "75-100.png")]
    public void VolumeBandsAndMute(float volume, string expected)
    {
        Assert.Equal(expected, Images.VolumeAsset(volume, false));
        Assert.Equal("mute.png", Images.VolumeAsset(volume, true));
    }

    private static void OnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private static byte[] Pixels(BitmapSource image)
    {
        var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
        var pixels = new byte[converted.PixelWidth * converted.PixelHeight * 4];
        converted.CopyPixels(pixels, converted.PixelWidth * 4, 0);
        return pixels;
    }

    [Fact]
    public void MissingAndMalformedDeviceIconsUseTheWindowsSpeakerIcon() =>
        OnSta(() =>
        {
            var windowsIcon = Assert.IsAssignableFrom<BitmapSource>(
                Native.DeviceIcon(Path.Combine(Environment.SystemDirectory, "mmres.dll") + ",-3010")
            );
            var expected = Pixels(windowsIcon);
            Assert.Equal(32, windowsIcon.PixelWidth);
            Assert.Equal(32, windowsIcon.PixelHeight);
            Assert.Contains(expected.Where((_, i) => i % 4 == 3), alpha => alpha != 0);
            foreach (
                var path in new[]
                {
                    "",
                    " ",
                    "missing-device-icon.dll,-999",
                    "\0",
                    "@\"missing.dll\",invalid",
                }
            )
            {
                var fallback = Assert.IsAssignableFrom<BitmapSource>(Native.DeviceIcon(path));
                Assert.True(fallback.IsFrozen);
                Assert.Equal(expected, Pixels(fallback));
            }

            var corruptIcon = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ico");
            try
            {
                File.WriteAllBytes(corruptIcon, [0, 0, 1, 0, 1, 0]);
                Assert.Equal(expected, Pixels((BitmapSource)Native.DeviceIcon(corruptIcon)));
            }
            finally
            {
                File.Delete(corruptIcon);
            }
        });

    [Fact]
    public void AssetsHaveConsistentBackgroundsAndNativeIconsAtEveryTraySize() =>
        OnSta(() =>
        {
            _ = new Application();
            var states = Assets.Select(Images.TrayAsset).ToArray();
            foreach (var state in states)
            {
                Assert.Equal(256, state.PixelWidth);
                Assert.Equal(256, state.PixelHeight);
                Assert.Equal(0, Pixels(state)[3]);
                foreach (var size in new[] { 16, 20, 24, 32, 40, 48, 64 })
                {
                    var resized = Images.Resize(state, size);
                    var pixels = Pixels(resized);
                    // Corners stay transparent, while the background fills each edge.
                    foreach (
                        var corner in new[] { 0, size - 1, size * (size - 1), size * size - 1 }
                    )
                    {
                        Assert.InRange(pixels[corner * 4 + 3], 0, 16);
                    }

                    foreach (
                        var edge in new[]
                        {
                            size / 2,
                            size * (size / 2),
                            size * (size / 2) + size - 1,
                            size * (size - 1) + size / 2,
                        }
                    )
                    {
                        Assert.Equal(255, pixels[edge * 4 + 3]);
                    }

                    var icon = Images.Icon(state, size);
                    Assert.NotEqual(0, icon);
                    Assert.True(DestroyIcon(icon));
                }
            }

            var silent = Pixels(states[0]);
            var low = Pixels(states[1]);
            for (var y = 0; y < 256; y++)
            {
                for (var x = 0; x < 256; x++)
                {
                    var offset = (y * 256 + x) * 4;
                    if (x < 128)
                    {
                        Assert.Equal(low[offset..(offset + 4)], silent[offset..(offset + 4)]);
                    }
                    else if (silent[offset + 3] == 255)
                    {
                        Assert.Equal(new byte[] { 88, 88, 88, 255 }, silent[offset..(offset + 4)]);
                    }
                }
            }

            for (var i = 1; i < 5; i++)
            {
                Assert.True(
                    Pixels(states[i]).Where((_, index) => index % 4 == 0).Sum(b => (long)b)
                        > Pixels(states[i - 1])
                            .Where((_, index) => index % 4 == 0)
                            .Sum(b => (long)b)
                );
            }

            var sheet = new DrawingVisual();
            using (var context = sheet.RenderOpen())
            {
                context.DrawRectangle(Brushes.WhiteSmoke, null, new Rect(0, 0, 480, 240));
                for (var i = 0; i < states.Length; i++)
                {
                    context.DrawImage(states[i], new Rect(i * 80 + 8, 8, 64, 64));
                    context.DrawImage(
                        Images.Resize(states[i], 16),
                        new Rect(i * 80 + 8, 88, 16, 16)
                    );
                    context.DrawImage(
                        Images.Resize(states[i], 24),
                        new Rect(i * 80 + 32, 84, 24, 24)
                    );
                    var tinted = Images.Tint(
                        states[i],
                        new DeviceSettings
                        {
                            Hue = 210,
                            Saturation = 75,
                            Brightness = -20,
                        }
                    );
                    context.DrawImage(tinted, new Rect(i * 80 + 8, 128, 64, 64));
                    context.DrawImage(Images.Resize(tinted, 24), new Rect(i * 80 + 8, 208, 24, 24));
                }
            }

            var rendered = new RenderTargetBitmap(480, 240, 96, 96, PixelFormats.Pbgra32);
            rendered.Render(sheet);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rendered));
            using var file = File.Create(
                Path.Combine(AppContext.BaseDirectory, "tray-icon-review.png")
            );
            encoder.Save(file);
        });

    [Fact]
    public void TintPreservesAlphaAndSupportsHueSaturationAndBrightness() =>
        OnSta(() =>
        {
            var source = BitmapSource.Create(
                1,
                1,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                new byte[] { 128, 128, 128, 123 },
                4
            );
            Assert.Same(source, Images.Tint(source, new DeviceSettings()));
            var red = Pixels(Images.Tint(source, new DeviceSettings { Saturation = 100 }));
            var green = Pixels(
                Images.Tint(source, new DeviceSettings { Hue = 120, Saturation = 100 })
            );
            var blue = Pixels(
                Images.Tint(source, new DeviceSettings { Hue = 240, Saturation = 100 })
            );
            Assert.True(red[2] > red[1] && red[2] > red[0]);
            Assert.True(green[1] > green[2] && green[1] > green[0]);
            Assert.True(blue[0] > blue[1] && blue[0] > blue[2]);
            Assert.Equal(
                red,
                Pixels(Images.Tint(source, new DeviceSettings { Hue = 360, Saturation = 100 }))
            );
            Assert.Equal(
                new byte[] { 255, 255, 255, 123 },
                Pixels(Images.Tint(source, new DeviceSettings { Brightness = 100 }))
            );
            Assert.Equal(
                new byte[] { 0, 0, 0, 123 },
                Pixels(Images.Tint(source, new DeviceSettings { Brightness = -100 }))
            );
            Assert.Equal(123, blue[3]);
        });
}
