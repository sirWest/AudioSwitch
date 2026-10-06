using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using AudioSwitch.Osd;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class OsdSkinTests
{
    public static TheoryData<string> MigratedSkins =>
        new()
        {
            "Ace",
            "Aero",
            "Classic PRO",
            "Cold",
            "Gnome",
            "HP",
            "HP ProBook",
            "Hybrid-X Cold",
            "LCD",
            "Mini Volume Bar",
            "Thoughtful Milk",
            "Ubunte",
            "Ubuntu 10",
            "VAIO",
            "Volume win8",
            "VU Meter"
        };

    [Theory]
    [MemberData(nameof(MigratedSkins))]
    public void MigratedSkinsArePackagedAsSingleThemeVersionTwo(string name)
    {
        Assert.Contains(name, OsdWindow.Skins);
        var directory = Path.Combine(OsdWindow.SkinRoot, name);
        var skin = XElement.Load(Path.Combine(directory, "skin.xml"));
        Assert.Equal("OSDskin", skin.Name.LocalName);
        Assert.Equal("2.0", (string?)skin.Attribute("Version"));
        Assert.Empty(skin.Descendants("Light"));
        Assert.Empty(skin.Descendants("Dark"));
        var background = Images.Load(
            OsdSkin.ImagePath(directory, skin, false, "Background", "back.png")
        );
        foreach (var state in new[] { "Background", "Mute", "Meter", "DeviceBackground" })
        {
            var fallback = state == "Background" ? "back.png" : state.ToLowerInvariant() + ".png";
            var path = OsdSkin.ImagePath(directory, skin, false, state, fallback);
            Assert.Equal(path, OsdSkin.ImagePath(directory, skin, true, state, fallback));
            var image = Images.Load(path);
            if (state != "Meter")
            {
                Assert.Equal(background.PixelWidth, image.PixelWidth);
                Assert.Equal(background.PixelHeight, image.PixelHeight);
                continue;
            }

            var bar = skin.Element("VolBar")!;
            var steps = (int)bar.Attribute("Steps")!;
            Assert.True(steps > 0);
            var height = image.PixelHeight;
            if ((string?)bar.Attribute("Type") == "bitstrip")
            {
                Assert.Equal(0, height % steps);
                height /= steps;
            }

            Assert.InRange((int)bar.Attribute("X")! + image.PixelWidth, 1, background.PixelWidth);
            Assert.InRange((int)bar.Attribute("Y")! + height, 1, background.PixelHeight);
        }

        var text = skin.Element("DeviceText")!;
        Assert.Equal(OsdSkin.TextColor(text, false), OsdSkin.TextColor(text, true));
        Assert.True((int)text.Attribute("X")! >= 0);
        Assert.True((int)text.Attribute("Y")! >= 0);
        Assert.True((int)text.Attribute("MaxWidth")! > 0);
        Assert.True((int)text.Attribute("MaxHeight")! > 0);
        Assert.Equal(
            OsdSkin.ImagePath(directory, skin, false, "Background", "back.png"),
            OsdSkin.ImagePath(directory, skin, false, "DeviceBackground", "back.png")
        );
    }

    [Fact]
    public void ThemeImagesFallBackPerPropertyAndCannotEscapeSkin()
    {
        var directory = Path.Combine(OsdWindow.SkinRoot, "Default");
        var skin = XElement.Parse(
            "<OSDskin><Images Background='shared.png'><Dark Background='dark.png'/></Images></OSDskin>"
        );
        Assert.Equal(
            Path.Combine(directory, "dark.png"),
            OsdSkin.ImagePath(directory, skin, true, "Background", "back.png")
        );
        Assert.Equal(
            Path.Combine(directory, "shared.png"),
            OsdSkin.ImagePath(directory, skin, false, "Background", "back.png")
        );
        Assert.Equal(
            Path.Combine(directory, "meter.png"),
            OsdSkin.ImagePath(directory, skin, true, "Meter", "meter.png")
        );
        skin.Element("Images")!.SetAttributeValue("Mute", "../outside.png");
        Assert.Throws<InvalidDataException>(() =>
            OsdSkin.ImagePath(directory, skin, false, "Mute", "mute.png")
        );
        var text = XElement.Parse("<DeviceText ColorHex='#123456' ColorHexDark='#ABCDEF'/>");
        Assert.Equal("#123456", OsdSkin.TextColor(text, false));
        Assert.Equal("#ABCDEF", OsdSkin.TextColor(text, true));
    }

    [Fact]
    public void AllSkinsRenderAndDefaultUpdatesWithResolvedAppTheme()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var window = new OsdWindow();
            try
            {
                var canvas = Assert.IsType<Canvas>(window.Content);
                var devicePreviews = new List<(string Name, bool Dark, BitmapSource Bitmap)>();
                var longDevicePreviews = new List<(string Name, bool Dark, BitmapSource Bitmap)>();
                Assert.Contains("Default (old)", OsdWindow.Skins);
                foreach (var name in OsdWindow.Skins)
                {
                    foreach (var dark in new[] { false, true })
                    {
                        window.Resources["SolidBackgroundFillColorBase"] = dark
                            ? Colors.Black
                            : Colors.White;
                        foreach (var volume in new[] { 0f, .01f, .5f, 1f })
                        {
                            window.Display(new OsdSettings { Skin = name }, volume, preview: true);
                            Assert.NotEmpty(canvas.Children.Cast<UIElement>());
                            if (!dark && volume == .5f)
                            {
                                SavePreview(canvas, name + "-volume");
                            }
                        }

                        window.Display(new OsdSettings { Skin = name }, muted: true, preview: true);
                        Assert.Single(canvas.Children.Cast<UIElement>());
                        if (!dark)
                        {
                            SavePreview(canvas, name + "-mute");
                        }

                        var skinSize = new Size(window.Width, window.Height);
                        Size? deviceSize = null;
                        foreach (
                            var (label, suffix) in new[]
                            {
                                ("Speakers", "short"),
                                ("Headphones (USB Audio Device)", "normal"),
                                (
                                    "Headphones (High Definition USB Audio Device with Surround Sound)",
                                    "long"
                                ),
                            }
                        )
                        {
                            window.Display(
                                new OsdSettings { Skin = name },
                                device: label,
                                preview: true
                            );
                            canvas.UpdateLayout();
                            var text = Assert.IsType<TextBlock>(canvas.Children[1]);
                            Assert.InRange(
                                Canvas.GetLeft(text),
                                0,
                                canvas.Width - text.ActualWidth
                            );
                            Assert.InRange(
                                Canvas.GetTop(text),
                                0,
                                canvas.Height - text.ActualHeight
                            );
                            Assert.InRange(text.ActualHeight, text.FontSize, text.MaxHeight);
                            var size = new Size(window.Width, window.Height);
                            if (deviceSize.HasValue)
                            {
                                Assert.Equal(deviceSize.Value, size);
                            }

                            deviceSize = size;
                            var theme = dark ? "dark" : "light";
                            var bitmap = SavePreview(canvas, $"{name}-device-{theme}-{suffix}");
                            if (suffix == "long")
                            {
                                longDevicePreviews.Add((name, dark, bitmap));
                            }

                            if (suffix == "normal")
                            {
                                devicePreviews.Add((name, dark, bitmap));
                                if (!dark)
                                {
                                    SavePreview(canvas, name + "-device");
                                }
                            }
                        }

                        window.Display(new OsdSettings { Skin = name }, .5f, preview: true);
                        Assert.Equal(skinSize, new Size(window.Width, window.Height));
                        window.Display(new OsdSettings { Skin = name }, muted: true, preview: true);
                        Assert.Equal(skinSize, new Size(window.Width, window.Height));
                    }
                }

                SaveDeviceGallery(devicePreviews);
                SaveDeviceGallery(longDevicePreviews, "long-");

                var settings = new OsdSettings();
                foreach (var dark in new[] { false, true, false })
                {
                    window.Resources["SolidBackgroundFillColorBase"] = dark
                        ? Colors.Black
                        : Colors.White;
                    foreach (var state in new[] { "volume", "mute", "device" })
                    {
                        window.Display(
                            settings,
                            .65f,
                            state == "mute",
                            state == "device" ? "Headphones (USB Audio Device)" : null,
                            true
                        );
                        Assert.Equal(200, window.Width);
                        Assert.Equal(40, window.Height);
                        if (state == "device")
                        {
                            var text = Assert.IsType<TextBlock>(canvas.Children[1]);
                            Assert.Equal(
                                dark ? "#FFF5F5F5" : "#FF202020",
                                Assert.IsType<SolidColorBrush>(text.Foreground).Color.ToString()
                            );
                        }

                        canvas.UpdateLayout();
                        var bitmap = new RenderTargetBitmap(200, 40, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(canvas);
                        var output = Path.Combine(AppContext.BaseDirectory, "osd-previews");
                        Directory.CreateDirectory(output);
                        using var file = File.Create(
                            Path.Combine(output, $"{(dark ? "dark" : "light")}-{state}.png")
                        );
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(file);
                    }
                }

                // A visible preview must recolor without another Display call.
                window.Resources["SolidBackgroundFillColorBase"] = Colors.Black;
                Assert.Equal(
                    "#FFF5F5F5",
                    ((SolidColorBrush)((TextBlock)canvas.Children[1]).Foreground).Color.ToString()
                );
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                window.EndPreview();
                window.Close();
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

    private static BitmapSource SavePreview(Canvas canvas, string name)
    {
        canvas.UpdateLayout();
        var bitmap = new RenderTargetBitmap(
            (int)canvas.Width,
            (int)canvas.Height,
            96,
            96,
            PixelFormats.Pbgra32
        );
        bitmap.Render(canvas);
        var output = Path.Combine(AppContext.BaseDirectory, "osd-previews");
        Directory.CreateDirectory(output);
        using var file = File.Create(Path.Combine(output, name + ".png"));
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(file);
        return bitmap;
    }

    private static void SaveDeviceGallery(
        List<(string Name, bool Dark, BitmapSource Bitmap)> previews,
        string prefix = ""
    )
    {
        foreach (var dark in new[] { false, true })
        {
            var page = 0;
            foreach (var group in previews.Where(p => p.Dark == dark).Chunk(10))
            {
                var visual = new DrawingVisual();
                using (var drawing = visual.RenderOpen())
                {
                    var background = new SolidColorBrush(
                        dark ? Color.FromRgb(32, 32, 32) : Color.FromRgb(245, 245, 245)
                    );
                    drawing.DrawRectangle(background, null, new Rect(0, 0, 1280, 1550));
                    for (var i = 0; i < group.Length; i++)
                    {
                        var x = i % 2 * 640 + 16;
                        var y = i / 2 * 310 + 12;
                        var label = new FormattedText(
                            group[i].Name,
                            CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"),
                            14,
                            dark ? Brushes.White : Brushes.Black,
                            1
                        );
                        drawing.DrawText(label, new Point(x, y));
                        var bitmap = group[i].Bitmap;
                        // Native pixel size keeps small labels honest during visual review.
                        drawing.DrawImage(
                            bitmap,
                            new Rect(x, y + 28, bitmap.PixelWidth, bitmap.PixelHeight)
                        );
                    }
                }

                var sheet = new RenderTargetBitmap(1280, 1550, 96, 96, PixelFormats.Pbgra32);
                sheet.Render(visual);
                var path = Path.Combine(
                    AppContext.BaseDirectory,
                    "osd-previews",
                    $"device-gallery-{prefix}{(dark ? "dark" : "light")}-{++page}.png"
                );
                using var file = File.Create(path);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(sheet));
                encoder.Save(file);
            }
        }
    }
}
