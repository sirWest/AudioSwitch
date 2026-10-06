using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AudioSwitch.Shell;

internal static partial class Native
{
    internal static MonitorInfo MonitorAt(Point point)
    {
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(MonitorFromPoint(point, 2), ref info))
        {
            throw new System.ComponentModel.Win32Exception();
        }

        return info;
    }

    internal static void Position(Window window, int x, int y)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        SetWindowPos(hwnd, 0, x, y, 0, 0, 0x15);
    }

    internal static void ClampToMonitor(Window window, int x, int y)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        var work = MonitorAt(new Point { X = x, Y = y }).Work;
        // Move first so WPF receives the destination monitor's DPI before measuring.
        Position(
            window,
            Math.Clamp(x, work.Left, work.Right - 1),
            Math.Clamp(y, work.Top, work.Bottom - 1)
        );
        window.UpdateLayout();
        GetWindowRect(hwnd, out var bounds);
        Position(
            window,
            Math.Clamp(
                x,
                work.Left,
                Math.Max(work.Left, work.Right - (bounds.Right - bounds.Left))
            ),
            Math.Clamp(y, work.Top, Math.Max(work.Top, work.Bottom - (bounds.Bottom - bounds.Top)))
        );
    }

    internal static void PositionOsd(Window window, int x, int y)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        // OSDs may cover the taskbar; only recover positions outside the physical display.
        var screen = MonitorAt(new Point { X = x, Y = y }).Monitor;
        Position(
            window,
            Math.Clamp(x, screen.Left, screen.Right - 1),
            Math.Clamp(y, screen.Top, screen.Bottom - 1)
        );
        window.UpdateLayout();
        GetWindowRect(hwnd, out var bounds);
        Position(
            window,
            Math.Clamp(
                x,
                screen.Left,
                Math.Max(screen.Left, screen.Right - (bounds.Right - bounds.Left))
            ),
            Math.Clamp(
                y,
                screen.Top,
                Math.Max(screen.Top, screen.Bottom - (bounds.Bottom - bounds.Top))
            )
        );
    }
}
