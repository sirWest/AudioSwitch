using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed partial class FlyoutAnimationTests
{
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(nint hwnd);

    [UiFact]
    public void HidingDuringAnimationDoesNotRestoreAVisibleNativeWindow()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = new Window
                {
                    Width = 280,
                    Height = 200,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                };
                Native.FlyoutAnimation? animation = null;
                var restoredWhileVisible = false;
                var visibilityNotifications = 0;
                window.IsVisibleChanged += (_, _) =>
                {
                    if (!window.IsVisible)
                    {
                        visibilityNotifications++;
                        animation?.Dispose();
                        animation = null;
                    }
                };
                try
                {
                    var handle = new WindowInteropHelper(window).EnsureHandle();
                    var source = HwndSource.FromHwnd(handle)!;
                    nint Hook(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
                    {
                        if (message == 0x0047 && !window.IsVisible) // WM_WINDOWPOSCHANGED
                        {
                            restoredWhileVisible |= IsWindowVisible(hwnd);
                        }
                        return 0;
                    }
                    source.AddHook(Hook);
                    try
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            Native.WithoutPresentation(
                                window,
                                () =>
                                {
                                    window.Show();
                                    Native.GetWindowRect(handle, out var initial);
                                    var work = Native
                                        .MonitorAt(new() { X = initial.Left, Y = initial.Top })
                                        .Work;
                                    var anchor = new Native.Rect
                                    {
                                        Left = work.Right - 40,
                                        Right = work.Right - 20,
                                        Top = work.Bottom,
                                        Bottom = work.Bottom + 20,
                                    };
                                    TrayIcon.Position(window, anchor);
                                    Native.GetWindowRect(handle, out var target);
                                    animation = new Native.FlyoutAnimation(window, anchor);
                                    Native.GetWindowRect(handle, out var staged);
                                    Assert.True(
                                        staged.Left != target.Left || staged.Top != target.Top,
                                        "An interactive desktop with a taskbar is required."
                                    );

                                    window.Hide();

                                    Assert.False(IsWindowVisible(handle));
                                    Native.GetWindowRect(handle, out var restored);
                                    Assert.Equal(target, restored);
                                }
                            );
                        }
                        Assert.Equal(10, visibilityNotifications);
                        Assert.False(
                            restoredWhileVisible,
                            "Animation cleanup restored the open position before the native window was hidden."
                        );
                    }
                    finally
                    {
                        source.RemoveHook(Hook);
                    }
                }
                finally
                {
                    window.Close();
                    animation?.Dispose();
                }
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
