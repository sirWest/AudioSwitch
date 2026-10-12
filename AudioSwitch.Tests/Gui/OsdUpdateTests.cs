using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using AudioSwitch.Core.Settings;
using AudioSwitch.Osd;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class OsdUpdateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RapidVolumeUpdatesKeepTheSurfaceAndNativeWindowStable(bool nearScreenEdge)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var window = new OsdWindow();
            try
            {
                var settings = new OsdSettings { Timeout = 100000 };
                window.Display(settings, .5f);
                if (nearScreenEdge)
                {
                    var screen = Native.MonitorAt(new() { X = 61, Y = 26 }).Monitor;
                    settings.Left = screen.Right - 1;
                    settings.Top = screen.Bottom - 1;
                    window.Display(settings, .5f);
                }
                var canvas = Assert.IsType<Canvas>(window.Content);
                var surface = canvas.Children[0];
                var styleChanges = 0;
                var positionChanges = 0;
                nint Hook(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
                {
                    if (message == 0x007C)
                        styleChanges++;
                    if (message == 0x0046)
                        positionChanges++;
                    return 0;
                }

                var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle)!;
                source.AddHook(Hook);
                try
                {
                    for (var i = 0; i < 100; i++)
                        window.Display(settings, (i % 21) / 20f);
                    Assert.Equal(0, styleChanges);
                    Assert.Equal(0, positionChanges);
                    Assert.Same(surface, canvas.Children[0]);
                    Assert.True(window.IsVisible);
                    Assert.Equal(settings.Opacity, window.Opacity);
                }
                finally
                {
                    source.RemoveHook(Hook);
                }

                window.Display(settings, muted: true);
                Assert.Single(canvas.Children);
                window.Display(settings, device: "Speakers");
                Assert.IsType<TextBlock>(canvas.Children[1]);
                window.Display(settings, .5f);
                Assert.All(canvas.Children.Cast<UIElement>(), child => Assert.IsType<Image>(child));
                Assert.Same(surface, canvas.Children[0]);
                window.Display(settings, preview: true);
                var handle = new WindowInteropHelper(window).Handle;
                Assert.Equal((nint)0, Native.GetWindowLongPtr(handle, -20) & 0x20);
                window.Display(settings);
                Assert.NotEqual((nint)0, Native.GetWindowLongPtr(handle, -20) & 0x20);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                window.Close();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
