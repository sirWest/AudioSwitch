using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioSwitch.Imaging;

namespace AudioSwitch.Flyout;

public sealed class VolumeThumb : FrameworkElement
{
    private static readonly BitmapSource Normal = Images.Asset("ThumbNormal.png");
    private static readonly BitmapSource Hover = Images.Asset("ThumbHover.png");
    private static readonly BitmapSource DarkNormal = Invert(Normal);
    private static readonly BitmapSource DarkHover = Invert(Hover);
    public static readonly DependencyProperty SurfaceProperty = DependencyProperty.Register(
        nameof(Surface),
        typeof(Brush),
        typeof(VolumeThumb),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender)
    );
    public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register(
        nameof(Active),
        typeof(bool),
        typeof(VolumeThumb),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
    );
    public Brush Surface
    {
        get => (Brush)GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }
    public bool Active
    {
        get => (bool)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    public VolumeThumb()
    {
        SetResourceReference(SurfaceProperty, "ApplicationBackgroundBrush");
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var dark =
            Surface is SolidColorBrush surface
            && (surface.Color.R * .2126 + surface.Color.G * .7152 + surface.Color.B * .0722) < 128;
        dc.DrawImage(
            dark
                ? Active
                    ? DarkHover
                    : DarkNormal
                : Active
                    ? Hover
                    : Normal,
            new Rect(RenderSize)
        );
    }

    private static BitmapSource Invert(BitmapSource source)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var stride = converted.PixelWidth * 4;
        var pixels = new byte[stride * converted.PixelHeight];
        converted.CopyPixels(pixels, stride, 0);
        // Preserve alpha so the rounded edges remain transparent in either theme.
        for (var i = 0; i < pixels.Length; i += 4)
        {
            for (var channel = 0; channel < 3; channel++)
            {
                pixels[i + channel] = (byte)(255 - pixels[i + channel]);
            }
        }

        var result = BitmapSource.Create(
            converted.PixelWidth,
            converted.PixelHeight,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            pixels,
            stride
        );
        result.Freeze();
        return result;
    }
}
