using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using AudioSwitch.Core.Placement;
using AudioSwitch.Imaging;

namespace AudioSwitch.Shell;

internal sealed class TrayIcon : IDisposable
{
    private readonly HwndSource source;
    private Native.NotifyData data;
    private readonly uint taskbarCreated = Native.RegisterWindowMessage("TaskbarCreated");
    private nint ownedIcon;
    internal event Action? LeftClick;
    internal event Action? RightClick;
    internal event Action<int>? Hotkey;
    internal event Action? DisplayChanged;
    internal nint Handle => source.Handle;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindow(string className, string? title);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint window);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetricsForDpi(int index, uint dpi);

    internal int IconSize
    {
        get
        {
            var dpi = GetDpiForWindow(FindWindow("Shell_TrayWnd", null));
            return Math.Max(16, GetSystemMetricsForDpi(49, dpi == 0 ? 96 : dpi));
        }
    }

    internal TrayIcon()
    {
        source = new HwndSource(
            new HwndSourceParameters("AudioSwitch messages")
            {
                Width = 0,
                Height = 0,
                WindowStyle = 0x00800000,
            }
        );
        source.AddHook(WndProc);
        ownedIcon = Images.Icon(Images.TrayAsset("0-25.png"), IconSize);
        data = new()
        {
            Size = (uint)Marshal.SizeOf<Native.NotifyData>(),
            Window = source.Handle,
            Id = 1,
            Flags = 1 | 2 | 4 | 0x80,
            Message = 0x8001,
            Icon = ownedIcon,
            Tip = "AudioSwitch",
            Info = "",
            InfoTitle = "",
        };
        Add();
    }

    private void Add()
    {
        Native.Shell_NotifyIcon(0, ref data);
        data.Version = 4;
        Native.Shell_NotifyIcon(4, ref data);
    }

    private nint WndProc(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == taskbarCreated)
        {
            Add();
        }

        if (message == taskbarCreated || message is 0x7e or 0x1a or 0x2e0)
        {
            DisplayChanged?.Invoke();
        }

        if (message == 0x312)
        {
            Hotkey?.Invoke((int)wParam);
            handled = true;
        }

        if (message == 0x8001)
        {
            var notification = (int)((long)lParam & 0xffff);
            if (notification is 0x400 or 0x401)
            {
                LeftClick?.Invoke();
            }

            if (notification == 0x7b)
            {
                RightClick?.Invoke();
            }

            handled = true;
        }

        return 0;
    }

    internal void Update(string text, nint icon = 0)
    {
        data.Tip = text.Length > 127 ? text[..127] : text;
        if (icon != 0)
        {
            var old = ownedIcon;
            data.Icon = ownedIcon = icon;
            if (old != 0)
            {
                Native.DestroyIcon(old);
            }
        }

        data.Flags = 1 | 2 | 4 | 0x80;
        Native.Shell_NotifyIcon(1, ref data);
    }

    internal void Notify(string text)
    {
        data.Info = text.Length > 255 ? text[..255] : text;
        data.InfoTitle = "AudioSwitch";
        data.InfoFlags = 1;
        data.Flags = 0x10;
        Native.Shell_NotifyIcon(1, ref data);
        data.Flags = 1 | 2 | 4 | 0x80;
    }

    internal Native.Rect Anchor()
    {
        var id = new Native.IconId
        {
            Size = (uint)Marshal.SizeOf<Native.IconId>(),
            Window = Handle,
            Id = 1,
        };
        if (Native.Shell_NotifyIconGetRect(ref id, out var rect) == 0)
        {
            return rect;
        }

        Native.GetCursorPos(out var cursor);
        return new()
        {
            Left = cursor.X,
            Right = cursor.X + 1,
            Top = cursor.Y,
            Bottom = cursor.Y + 1,
        };
    }

    internal void Position(Window window) => Position(window, Anchor());

    internal static void Position(Window window, Native.Rect anchor)
    {
        var info = Native.MonitorAt(new() { X = anchor.Left, Y = anchor.Top });
        var handle = new WindowInteropHelper(window).EnsureHandle();
        var targetMonitor = Native.MonitorFromPoint(new() { X = anchor.Left, Y = anchor.Top }, 2);
        if (Native.MonitorFromWindow(handle, 2) != targetMonitor)
        {
            // Stage on the target monitor only when DPI may change. Cloaking an
            // already-positioned flyout on every audio notification causes flicker.
            Native.WithoutPresentation(
                window,
                () =>
                {
                    Native.Position(window, info.Work.Left, info.Work.Top);
                    Place();
                }
            );
            return;
        }

        Place();

        void Place()
        {
            window.UpdateLayout();
            Native.GetWindowRect(handle, out var bounds);
            var size = new Size(bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
            var position = WindowPlacement.NearAnchor(
                new(
                    anchor.Left,
                    anchor.Top,
                    anchor.Right - anchor.Left,
                    anchor.Bottom - anchor.Top
                ),
                new(
                    info.Work.Left,
                    info.Work.Top,
                    info.Work.Right - info.Work.Left,
                    info.Work.Bottom - info.Work.Top
                ),
                size.Width,
                size.Height,
                8 * System.Windows.Media.VisualTreeHelper.GetDpi(window).DpiScaleX
            );
            if (bounds.Left != (int)position.X || bounds.Top != (int)position.Y)
            {
                Native.Position(window, (int)position.X, (int)position.Y);
            }
        }
    }

    internal void ShowMenu(ContextMenu menu)
    {
        Native.SetForegroundWindow(Handle);
        menu.IsOpen = true;
    }

    public void Dispose()
    {
        Native.Shell_NotifyIcon(2, ref data);
        source.Dispose();
        if (ownedIcon != 0)
        {
            Native.DestroyIcon(ownedIcon);
        }
    }
}
