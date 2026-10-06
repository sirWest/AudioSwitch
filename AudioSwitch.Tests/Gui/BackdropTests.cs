using System.Windows;
using System.Windows.Media;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class BackdropTests
{
    [Theory]
    [InlineData(16299, true, false, false, true, false)]
    [InlineData(17134, true, false, false, true, true)]
    [InlineData(19045, true, false, false, true, true)]
    [InlineData(19045, false, false, false, true, false)]
    [InlineData(19045, true, true, false, true, false)]
    [InlineData(19045, true, false, true, true, false)]
    [InlineData(19045, true, false, false, false, false)]
    [InlineData(22000, true, false, false, true, true)]
    [InlineData(22621, true, false, false, true, true)]
    [InlineData(26100, true, false, false, true, true)]
    [InlineData(26100, false, false, false, true, false)]
    [InlineData(26100, true, true, false, true, false)]
    [InlineData(26100, true, false, true, true, false)]
    [InlineData(26100, true, false, false, false, false)]
    public void BackdropRequiresSupportedWindowsAndAvailableEffects(
        int build,
        bool transparency,
        bool highContrast,
        bool energySaver,
        bool composition,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            Native.CanUseBackdrop(build, transparency, highContrast, energySaver, composition)
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingNativeBackdropUsesOpaqueThemeColor(bool dark)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var window = new Window { Background = Brushes.Transparent };
                try
                {
                    var palette = dark
                        ? Color.FromArgb(80, 32, 32, 32)
                        : Color.FromArgb(80, 243, 243, 243);
                    window.Resources["SolidBackgroundFillColorBase"] = palette;
                    Native.ApplyWindowsBackdrop(window, dark);
                    var background = Assert.IsType<SolidColorBrush>(window.Background);
                    var expected = SystemParameters.HighContrast
                        ? SystemColors.WindowColor
                        : palette;
                    Assert.Equal(255, background.Color.A);
                    Assert.Equal(expected.R, background.Color.R);
                    Assert.Equal(expected.G, background.Color.G);
                    Assert.Equal(expected.B, background.Color.B);
                    Assert.Equal(1, background.Opacity);
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
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
