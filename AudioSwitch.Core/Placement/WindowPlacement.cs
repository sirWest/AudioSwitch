namespace AudioSwitch.Core.Placement;

public readonly record struct ScreenRect(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
}

public static class WindowPlacement
{
    public static (double X, double Y) NearAnchor(
        ScreenRect anchor,
        ScreenRect work,
        double width,
        double height,
        double gap = 8
    )
    {
        double x = anchor.X + anchor.Width / 2 - width / 2;
        double y = Math.Min(anchor.Y, work.Bottom) - height - gap;
        if (anchor.Bottom <= work.Y)
        {
            y = work.Y + gap;
        }
        else if (anchor.Right <= work.X)
        {
            x = work.X + gap;
            y = anchor.Y + anchor.Height / 2 - height / 2;
        }
        else if (anchor.X >= work.Right)
        {
            x = work.Right - width - gap;
            y = anchor.Y + anchor.Height / 2 - height / 2;
        }

        var marginX = Math.Min(gap, Math.Max(0, (work.Width - width) / 2));
        var marginY = Math.Min(gap, Math.Max(0, (work.Height - height) / 2));
        return (
            Math.Clamp(
                x,
                work.X + marginX,
                Math.Max(work.X + marginX, work.Right - width - marginX)
            ),
            Math.Clamp(
                y,
                work.Y + marginY,
                Math.Max(work.Y + marginY, work.Bottom - height - marginY)
            )
        );
    }
}
