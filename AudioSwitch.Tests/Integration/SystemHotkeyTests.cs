using System.Runtime.InteropServices;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class SystemHotkeyTests
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint window, int id, uint modifiers, uint key);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(nint window, int id);

    [SystemFact]
    public void WindowsRejectsDuplicateGlobalShortcutAndReleasesRegistration()
    {
        const uint modifiers = 1 | 2 | 4 | 0x4000;
        Assert.True(
            RegisterHotKey(0, 0x3101, modifiers, 0x87),
            "Ctrl+Alt+Shift+F24 is unavailable on this system."
        );
        try
        {
            Assert.False(RegisterHotKey(0, 0x3102, modifiers, 0x87));
        }
        finally
        {
            UnregisterHotKey(0, 0x3101);
            UnregisterHotKey(0, 0x3102);
        }

        Assert.True(RegisterHotKey(0, 0x3102, modifiers, 0x87));
        Assert.True(UnregisterHotKey(0, 0x3102));
    }
}
