using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace AudioSwitch.Shell;

internal static partial class Native
{
    [DllImport("gdi32.dll")]
    private static extern nint CreateRectRgn(int left, int top, int right, int bottom);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(nint hwnd, nint region, bool redraw);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindowEx(
        nint parent,
        nint after,
        string className,
        string? title
    );

    private static nint TaskbarAt(MonitorInfo monitor, out Rect bounds)
    {
        foreach (var className in new[] { "Shell_TrayWnd", "Shell_SecondaryTrayWnd" })
        {
            nint handle = 0;
            while ((handle = FindWindowEx(0, handle, className, null)) != 0)
            {
                if (
                    GetWindowRect(handle, out bounds)
                    && bounds.Right > monitor.Monitor.Left
                    && bounds.Left < monitor.Monitor.Right
                    && bounds.Bottom > monitor.Monitor.Top
                    && bounds.Top < monitor.Monitor.Bottom
                )
                {
                    return handle;
                }
            }
        }

        bounds = default;
        return 0;
    }

    internal sealed class FlyoutAnimation : IDisposable
    {
        private readonly Window window;
        private readonly nint hwnd;
        private readonly Rect target;
        private readonly Rect clip;
        private readonly nint taskbar;
        private readonly int offsetX;
        private readonly int offsetY;
        private readonly bool closing;
        private readonly Action? completed;
        private readonly Stopwatch clock = new();
        private readonly System.Windows.Threading.DispatcherTimer timer;
        private readonly KeySpline entrance = new(0, 0, 0, 1);
        private readonly KeySpline exit = new(1, 0, 1, 1);
        private bool finished;
        private bool prepared;

        internal FlyoutAnimation(
            Window window,
            Rect anchor,
            bool closing = false,
            Action? completed = null
        )
        {
            this.window = window;
            this.closing = closing;
            this.completed = completed;
            hwnd = new WindowInteropHelper(window).Handle;
            timer = new(System.Windows.Threading.DispatcherPriority.Render, window.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(16),
            };
            timer.Tick += Render;
            if (!GetWindowRect(hwnd, out target))
            {
                return;
            }

            var monitor = MonitorAt(new Point { X = anchor.Left, Y = anchor.Top });
            taskbar = TaskbarAt(monitor, out var bar);
            if (taskbar == 0)
            {
                return;
            }

            clip = monitor.Work;
            if (bar.Right - bar.Left >= bar.Bottom - bar.Top)
            {
                if (
                    Math.Abs(bar.Top - monitor.Monitor.Top)
                    < Math.Abs(bar.Bottom - monitor.Monitor.Bottom)
                )
                {
                    clip.Top = Math.Max(clip.Top, bar.Bottom);
                    offsetY = clip.Top - target.Bottom;
                }
                else
                {
                    clip.Bottom = Math.Min(clip.Bottom, bar.Top);
                    offsetY = clip.Bottom - target.Top;
                }
            }
            else if (
                Math.Abs(bar.Left - monitor.Monitor.Left)
                < Math.Abs(bar.Right - monitor.Monitor.Right)
            )
            {
                clip.Left = Math.Max(clip.Left, bar.Right);
                offsetX = clip.Left - target.Right;
            }
            else
            {
                clip.Right = Math.Min(clip.Right, bar.Left);
                offsetX = clip.Right - target.Left;
            }

            // Prepare while cloaked; keep the frame and its shadow behind the taskbar.
            prepared = Frame(closing ? 1 : 0);
            if (!prepared)
            {
                Restore();
            }
        }

        internal void Start()
        {
            if (finished || clock.IsRunning)
            {
                return;
            }

            if (!prepared || !SystemParameters.ClientAreaAnimation)
            {
                Complete();
                return;
            }

            clock.Start();
            timer.Start();
        }

        private void Render(object? sender, EventArgs e)
        {
            var progress = Math.Clamp(
                clock.Elapsed.TotalMilliseconds / (closing ? 167 : 250),
                0,
                1
            );
            if (!window.IsVisible || !SystemParameters.ClientAreaAnimation || progress >= 1)
            {
                Complete();
                return;
            }

            var visible = closing
                ? 1 - exit.GetSplineProgress(progress)
                : entrance.GetSplineProgress(progress);
            if (!Frame(visible))
            {
                Complete();
            }
        }

        private bool Frame(double visible)
        {
            var x = target.Left + (int)Math.Round(offsetX * (1 - visible));
            var y = target.Top + (int)Math.Round(offsetY * (1 - visible));
            var width = target.Right - target.Left;
            var height = target.Bottom - target.Top;
            var region = CreateRectRgn(
                Math.Clamp(clip.Left - x, 0, width),
                Math.Clamp(clip.Top - y, 0, height),
                Math.Clamp(clip.Right - x, 0, width),
                Math.Clamp(clip.Bottom - y, 0, height)
            );
            if (region == 0)
            {
                return false;
            }

            if (SetWindowRgn(hwnd, region, true) == 0)
            {
                DeleteObject(region);
                return false;
            }

            // Windows owns the region. SWP_NOACTIVATE preserves the foreground handoff.
            return SetWindowPos(hwnd, taskbar, x, y, 0, 0, 0x11);
        }

        private void Complete()
        {
            // Hide an exiting window before clearing its clip or restoring its position.
            try
            {
                completed?.Invoke();
            }
            finally
            {
                Dispose();
            }
        }

        private void Restore()
        {
            if (taskbar == 0)
            {
                return;
            }

            SetWindowPos(
                hwnd,
                window.Topmost ? new nint(-1) : new nint(-2),
                target.Left,
                target.Top,
                0,
                0,
                0x11
            );
            SetWindowRgn(hwnd, 0, true);
        }

        public void Dispose()
        {
            if (finished)
            {
                return;
            }

            finished = true;
            timer.Stop();
            timer.Tick -= Render;
            clock.Stop();
            Restore();
        }
    }
}
