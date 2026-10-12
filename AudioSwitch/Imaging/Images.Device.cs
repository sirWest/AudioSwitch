using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AudioSwitch.Imaging;

internal static partial class Images
{
    internal static BitmapSource DeviceTray(BitmapSource source, bool muted, int size)
    {
        var visual = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);
        using (var context = visual.RenderOpen())
        {
            var scale = (double)size / Math.Max(source.PixelWidth, source.PixelHeight);
            var width = source.PixelWidth * scale;
            var height = source.PixelHeight * scale;
            context.DrawImage(
                source,
                new Rect((size - width) / 2, (size - height) / 2, width, height)
            );
            if (muted)
            {
                context.PushTransform(new ScaleTransform(size, size));
                context.DrawRoundedRectangle(
                    new SolidColorBrush(Color.FromRgb(250, 250, 250)),
                    new Pen(new SolidColorBrush(Color.FromRgb(110, 115, 120)), .035),
                    new Rect(.28, .51, .70, .47),
                    .08,
                    .08
                );
                var speaker = Geometry.Parse(
                    "M .36,.67 L .46,.67 .60,.58 .60,.91 .46,.82 .36,.82 Z"
                );
                context.DrawGeometry(new SolidColorBrush(Color.FromRgb(35, 39, 43)), null, speaker);
                var red = new Pen(new SolidColorBrush(Color.FromRgb(213, 34, 53)), .085)
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round,
                };
                context.DrawLine(red, new Point(.72, .66), new Point(.88, .83));
                context.DrawLine(red, new Point(.88, .66), new Point(.72, .83));
                context.Pop();
            }
        }
        var result = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        result.Render(visual);
        result.Freeze();
        return result;
    }
}
