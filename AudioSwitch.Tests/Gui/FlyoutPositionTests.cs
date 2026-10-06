using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Interop;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class FlyoutPositionTests
{
    [UiFact]
    public void RepeatedPositioningDoesNotMoveTheWindowAndResizingStillAnchorsIt()
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
                try
                {
                    window.Show();
                    var handle = new WindowInteropHelper(window).Handle;
                    Native.GetWindowRect(handle, out var initial);
                    var work = Native.MonitorAt(new() { X = initial.Left, Y = initial.Top }).Work;
                    var anchor = new Native.Rect
                    {
                        Left = work.Right - 40,
                        Right = work.Right - 20,
                        Top = work.Bottom,
                        Bottom = work.Bottom + 20,
                    };
                    TrayIcon.Position(window, anchor);
                    Native.GetWindowRect(handle, out var positioned);

                    var positionChanges = 0;
                    nint Hook(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
                    {
                        if (message == 0x0046) // WM_WINDOWPOSCHANGING
                        {
                            positionChanges++;
                        }
                        return 0;
                    }

                    var source = HwndSource.FromHwnd(handle)!;
                    source.AddHook(Hook);
                    try
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            TrayIcon.Position(window, anchor);
                        }
                        Assert.Equal(0, positionChanges);
                    }
                    finally
                    {
                        source.RemoveHook(Hook);
                    }

                    window.Height += 50;
                    TrayIcon.Position(window, anchor);
                    Native.GetWindowRect(handle, out var resized);
                    Assert.True(resized.Top < positioned.Top);
                    Assert.Equal(positioned.Bottom, resized.Bottom);
                    Assert.Equal(positioned.Right, resized.Right);
                }
                finally
                {
                    window.Close();
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
