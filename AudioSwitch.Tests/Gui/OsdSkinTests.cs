using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using AudioSwitch.Core.Settings;
using AudioSwitch.Osd;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class OsdSkinTests
{
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
    public void DefaultSkinRendersAndUpdatesWithResolvedAppTheme() =>
        RenderSkins(["Default"], verifyDefaultTheme: true);

    [SkinFact]
    [Trait("Category", "AdditionalSkins")]
    public void AdditionalSkinsRender()
    {
        var skins = OsdWindow.Skins.Where(name => name != "Default").ToArray();
        Assert.NotEmpty(skins);
        RenderSkins(skins, verifyDefaultTheme: false);
    }

    private static void RenderSkins(IEnumerable<string> skins, bool verifyDefaultTheme)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var window = new OsdWindow();
            try
            {
                var canvas = Assert.IsType<Canvas>(window.Content);
                Assert.Contains("Default", OsdWindow.Skins);
                foreach (var name in skins)
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
                            Render(canvas);
                        }

                        window.Display(new OsdSettings { Skin = name }, muted: true, preview: true);
                        Assert.Single(canvas.Children.Cast<UIElement>());
                        Render(canvas);

                        var skinSize = new Size(window.Width, window.Height);
                        Size? deviceSize = null;
                        foreach (
                            var label in new[]
                            {
                                "Speakers",
                                "Headphones (USB Audio Device)",
                                "Headphones (High Definition USB Audio Device with Surround Sound)",
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
                            // Layout rounding can exceed MaxHeight by less than one physical pixel.
                            var roundingTolerance = 1 / VisualTreeHelper.GetDpi(text).DpiScaleY;
                            Assert.InRange(
                                text.ActualHeight,
                                text.FontSize,
                                text.MaxHeight + roundingTolerance
                            );
                            var size = new Size(window.Width, window.Height);
                            if (deviceSize.HasValue)
                            {
                                AssertRoundedSize(deviceSize.Value, size, window);
                            }

                            deviceSize = size;
                            Render(canvas);
                        }

                        window.Display(new OsdSettings { Skin = name }, .5f, preview: true);
                        AssertRoundedSize(skinSize, new Size(window.Width, window.Height), window);
                        window.Display(new OsdSettings { Skin = name }, muted: true, preview: true);
                        AssertRoundedSize(skinSize, new Size(window.Width, window.Height), window);
                    }
                }

                if (!verifyDefaultTheme)
                {
                    return;
                }

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
                        AssertRoundedSize(
                            new Size(200, 40),
                            new Size(window.Width, window.Height),
                            window
                        );
                        if (state == "device")
                        {
                            var text = Assert.IsType<TextBlock>(canvas.Children[1]);
                            Assert.Equal(
                                dark ? "#FFF5F5F5" : "#FF202020",
                                Assert.IsType<SolidColorBrush>(text.Foreground).Color.ToString()
                            );
                        }

                        Render(canvas);
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

    private static void AssertRoundedSize(Size expected, Size actual, Visual visual)
    {
        // Showing a window can round its requested size to the display's physical pixels.
        var dpi = VisualTreeHelper.GetDpi(visual);
        Assert.InRange(Math.Abs(actual.Width - expected.Width), 0, 1 / dpi.DpiScaleX);
        Assert.InRange(Math.Abs(actual.Height - expected.Height), 0, 1 / dpi.DpiScaleY);
    }

    private static void Render(Canvas canvas)
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
    }
}
