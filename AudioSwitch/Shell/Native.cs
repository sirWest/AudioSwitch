using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AudioSwitch.Shell;

internal static partial class Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        public int X,
            Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left,
            Top,
            Right,
            Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        public int Size;
        public Rect Monitor,
            Work;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IconId
    {
        public uint Size;
        public nint Window;
        public uint Id;
        public Guid Guid;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NotifyData
    {
        public uint Size;
        public nint Window;
        public uint Id,
            Flags,
            Message;
        public nint Icon;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Tip;
        public uint State,
            StateMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Info;
        public uint Version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string InfoTitle;
        public uint InfoFlags;
        public Guid Guid;
        public nint BalloonIcon;
    }

    // Inline ByValTStr fields require the runtime marshaller.
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool Shell_NotifyIcon(uint message, ref NotifyData data);

    [LibraryImport("shell32.dll")]
    internal static partial int Shell_NotifyIconGetRect(ref IconId id, out Rect rect);

    [LibraryImport(
        "user32.dll",
        EntryPoint = "RegisterWindowMessageW",
        StringMarshalling = StringMarshalling.Utf16
    )]
    internal static partial uint RegisterWindowMessage(string name);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetCursorPos(out Point point);

    [LibraryImport("user32.dll")]
    internal static partial nint MonitorFromPoint(Point point, uint flags);

    [LibraryImport("user32.dll")]
    internal static partial nint MonitorFromWindow(nint window, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowPos(
        nint hwnd,
        nint after,
        int x,
        int y,
        int width,
        int height,
        uint flags
    );

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetWindowRect(nint hwnd, out Rect rect);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetForegroundWindow(nint hwnd);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrA")]
    internal static partial nint GetWindowLongPtr(nint hwnd, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrA")]
    internal static partial nint SetWindowLongPtr(nint hwnd, int index, nint value);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DestroyIcon(nint icon);

    [LibraryImport(
        "shell32.dll",
        EntryPoint = "ExtractIconExW",
        StringMarshalling = StringMarshalling.Utf16
    )]
    internal static partial uint ExtractIconEx(
        string file,
        int index,
        out nint large,
        out nint small,
        uint count
    );

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hwnd, int id, uint modifiers, uint key);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hwnd, int id);

    [LibraryImport("user32.dll")]
    internal static partial short GetAsyncKeyState(int key);

    internal delegate nint HookProc(int code, nint message, nint data);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExA", SetLastError = true)]
    internal static partial nint SetWindowsHookEx(
        int hook,
        HookProc callback,
        nint module,
        uint thread
    );

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnhookWindowsHookEx(nint hook);

    [LibraryImport("user32.dll")]
    internal static partial nint CallNextHookEx(nint hook, int code, nint message, nint data);

    [LibraryImport(
        "kernel32.dll",
        EntryPoint = "GetModuleHandleW",
        StringMarshalling = StringMarshalling.Utf16
    )]
    internal static partial nint GetModuleHandle(string? module);

    private static readonly Lazy<ImageSource> defaultDeviceIcon = new(() =>
        TryDeviceIcon(Path.Combine(Environment.SystemDirectory, "mmres.dll") + ",-3010")
        ?? Imaging.Images.Asset("0-25.png")
    );

    internal static ImageSource DeviceIcon(string path) =>
        TryDeviceIcon(path) ?? defaultDeviceIcon.Value;

    private static ImageSource? TryDeviceIcon(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        nint large = 0;
        nint small = 0;
        try
        {
            path = Environment.ExpandEnvironmentVariables(path);
            var comma = path.LastIndexOf(',');
            var index = 0;
            if (comma >= 0 && int.TryParse(path[(comma + 1)..], out index))
            {
                path = path[..comma];
            }

            path = path.Trim().Trim('"', '@');
            ExtractIconEx(path, index, out large, out small, 1);
            if (large == 0 && small == 0)
            {
                return null;
            }

            var bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                large != 0 ? large : small,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32)
            );
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception ex)
            when (ex
                    is ArgumentException
                        or ExternalException
                        or IOException
                        or UnauthorizedAccessException
                        or NotSupportedException
                        or InvalidOperationException
            )
        {
            System.Diagnostics.Trace.TraceWarning($"Audio device icon unavailable: {ex.Message}");
            return null;
        }
        finally
        {
            if (large != 0)
            {
                DestroyIcon(large);
            }

            if (small != 0 && small != large)
            {
                DestroyIcon(small);
            }
        }
    }
}
