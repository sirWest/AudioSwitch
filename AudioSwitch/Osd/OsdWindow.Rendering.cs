using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace AudioSwitch.Osd;

internal sealed partial class OsdWindow
{
    private int imageIndex;

    private void Image(BitmapSource source, double x = 0, double y = 0)
    {
        if (!source.IsFrozen)
        {
            source.Freeze();
        }
        Image image;
        if (imageIndex < canvas.Children.Count && canvas.Children[imageIndex] is Image existing)
        {
            image = existing;
        }
        else
        {
            image = new Image { Stretch = Stretch.Fill, IsHitTestVisible = false };
            if (imageIndex < canvas.Children.Count)
            {
                canvas.Children.RemoveAt(imageIndex);
            }
            canvas.Children.Insert(imageIndex, image);
        }
        imageIndex++;
        image.Source = source;
        image.Width = source.PixelWidth;
        image.Height = source.PixelHeight;
        image.Clip = null;
        Canvas.SetLeft(image, x);
        Canvas.SetTop(image, y);
    }

    private void TrimImages()
    {
        if (canvas.Children.Count > imageIndex)
        {
            canvas.Children.RemoveRange(imageIndex, canvas.Children.Count - imageIndex);
        }
    }

    private void Render(float volume, bool muted, string? device)
    {
        lastVolume = volume;
        lastMuted = muted;
        lastDevice = device;
        // Keep the background and volume controls attached during rapid updates.
        imageIndex = 0;
        var surface =
            device is not null && deviceBackground is not null ? deviceBackground
            : muted ? mute!
            : background!;
        Width = canvas.Width = surface.PixelWidth;
        Height = canvas.Height = surface.PixelHeight;
        Image(surface);
        if (device is not null)
        {
            TrimImages();
            RenderDeviceLabel(device);
            return;
        }

        if (muted)
        {
            TrimImages();
            return;
        }

        RenderVolume(volume);
        TrimImages();
    }

    private void RenderVolume(float volume)
    {
        var bar = skin!.Element("VolBar")!;
        var steps = (int)bar.Attribute("Steps")!;
        var x = (int?)bar.Attribute("X") ?? 0;
        var y = (int?)bar.Attribute("Y") ?? 0;
        var step = Math.Round(Math.Clamp(volume, 0, 1) * steps);
        if ((string?)bar.Attribute("Type") == "horizontal")
        {
            var width = (int)(step * meter!.PixelWidth / steps);
            if (width > 0)
            {
                Image(
                    new CroppedBitmap(
                        meter,
                        new Int32Rect(0, 0, Math.Min(width, meter.PixelWidth), meter.PixelHeight)
                    ),
                    x,
                    y
                );
                var radius = (double?)bar.Attribute("CornerRadius") ?? 0;
                if (radius > 0)
                {
                    canvas.Children[imageIndex - 1].Clip = new RectangleGeometry(
                        new Rect(0, 0, width, meter.PixelHeight),
                        radius,
                        radius
                    );
                }
            }

            if (effect is not null)
            {
                Image(
                    effect,
                    x + width - step * effect.PixelWidth / steps,
                    y + (meter.PixelHeight - effect.PixelHeight) / 2d
                );
            }
        }
        else
        {
            var height = meter!.PixelHeight / steps;
            if (height < 1)
            {
                throw new InvalidDataException("Invalid sprite strip.");
            }

            var top = Math.Min((int)step * height, meter.PixelHeight - height);
            Image(new CroppedBitmap(meter, new Int32Rect(0, top, meter.PixelWidth, height)), x, y);
        }
    }

    private void RenderDeviceLabel(string device)
    {
        var element =
            skin!.Element("DeviceText") ?? throw new InvalidDataException("Missing device text.");
        var text = new TextBlock
        {
            Text = device,
            FontFamily = new FontFamily((string?)element.Attribute("Font") ?? "Segoe UI"),
            FontSize = ((double?)element.Attribute("FontSize") ?? 9) * 96 / 72,
            Foreground = (Brush)
                new BrushConverter().ConvertFromString(OsdSkin.TextColor(element, loadedDark))!,
            Background = element.Attribute("BackgroundHex") is { } textBackground
                ? (Brush)new BrushConverter().ConvertFromString(textBackground.Value)!
                : Brushes.Transparent,
            Padding = new Thickness((double?)element.Attribute("Padding") ?? 0),
            Width = (double?)element.Attribute("MaxWidth") ?? Width,
            TextWrapping =
                (bool?)element.Attribute("Wrap") == false ? TextWrapping.NoWrap : TextWrapping.Wrap,
            TextAlignment = Enum.TryParse<TextAlignment>(
                (string?)element.Attribute("Alignment"),
                out var alignment
            )
                ? alignment
                : TextAlignment.Left,
            TextTrimming = TextTrimming.None,
            UseLayoutRounding = true,
            IsHitTestVisible = false,
        };
        if ((bool?)element.Attribute("Shadow") == true)
        {
            text.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 3,
                ShadowDepth = 1,
                Opacity = 1,
            };
        }

        var textX = (double?)element.Attribute("X") ?? 0;
        var textY = (double?)element.Attribute("Y") ?? 0;
        var maxHeight = (double?)element.Attribute("MaxHeight") ?? Height;
        // Keep the configured label area stable, including labels beside compact artwork.
        if (textX + text.Width > Width)
        {
            Width = canvas.Width = textX + text.Width + 6;
        }

        if (textY + maxHeight > Height)
        {
            Height = canvas.Height = textY + maxHeight + 6;
        }
        // Measure all lines before limiting height so WPF trims only the last visible line.
        text.Measure(new Size(text.Width, double.PositiveInfinity));
        var vertical = (string?)element.Attribute("VerticalAlignment");
        var spare = Math.Max(0, maxHeight - Math.Ceiling(text.DesiredSize.Height) - 1);
        var inset =
            vertical == "Center" ? spare / 2
            : vertical == "Bottom" ? spare
            : 0;
        text.Padding = new Thickness(
            text.Padding.Left,
            text.Padding.Top + inset,
            text.Padding.Right,
            text.Padding.Bottom
        );
        text.Height = text.MaxHeight = maxHeight;
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        Canvas.SetLeft(text, textX);
        Canvas.SetTop(text, textY);
        canvas.Children.Add(text);
    }
}
