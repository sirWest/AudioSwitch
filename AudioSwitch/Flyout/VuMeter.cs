using System.Windows;
using System.Windows.Media;

namespace AudioSwitch.Flyout;

public sealed class VuMeter : FrameworkElement
{
    public static readonly DependencyProperty SurfaceProperty = DependencyProperty.Register(
        nameof(Surface),
        typeof(Brush),
        typeof(VuMeter),
        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender)
    );
    public Brush Surface
    {
        get => (Brush)GetValue(SurfaceProperty);
        set => SetValue(SurfaceProperty, value);
    }

    public VuMeter() => SetResourceReference(SurfaceProperty, "ApplicationBackgroundBrush");

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(VuMeter),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender)
    );
    public static readonly DependencyProperty ColorfulProperty = DependencyProperty.Register(
        nameof(Colorful),
        typeof(bool),
        typeof(VuMeter),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
    );
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public bool Colorful
    {
        get => (bool)GetValue(ColorfulProperty);
        set => SetValue(ColorfulProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var count = 14;
        var width = Math.Max(0, (ActualWidth - (count - 1) * 2) / count);
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        var dark =
            Surface is SolidColorBrush surface
            && (surface.Color.R * .2126 + surface.Color.G * .7152 + surface.Color.B * .0722) < 128;
        for (var i = 0; i < count; i++)
        {
            var on = i < Math.Ceiling(Math.Clamp(Value, 0, 1) * count);
            var brush = on == dark ? Brushes.LightGray : Brushes.Gray;
            if (Colorful)
            {
                brush = i switch
                {
                    < 10 => on ? Brushes.Lime : Brushes.DarkGreen,
                    < 13 => on ? Brushes.Yellow : Brushes.Olive,
                    _ => on ? Brushes.Red : Brushes.Maroon,
                };
            }
            dc.DrawRectangle(brush, null, new Rect(i * (width + 2), 0, width, ActualHeight));
        }
    }
}
