using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace AudioSwitch.Shell;

internal static partial class Native
{
    private const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const uint DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const uint DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const uint DWMWA_BORDER_COLOR = 34;
    private const int DWMWA_COLOR_DEFAULT = unchecked((int)0xFFFFFFFF);

    private enum DwmWindowCornerPreference
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3,
    }

    private enum DwmSystemBackdropType
    {
        Auto = 0,
        None = 1,
        MainWindow = 2,
        TransientWindow = 3,
        TabbedWindow = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int State;
        public int Flags;
        public uint GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public nint Data;
        public nuint Size;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowCompositionAttribute(
        nint hwnd,
        ref WindowCompositionAttributeData data
    );

    private static bool ApplyLegacyAcrylic(nint hwnd, bool enabled, Color tint)
    {
        // Windows 10 has no system-backdrop attribute. Its accent policy uses ABGR.
        // Together with the flyout's surface brushes, this gives a subtle glass tint.
        var policy = new AccentPolicy
        {
            State = enabled ? 4 : 0, // ACCENT_ENABLE_ACRYLICBLURBEHIND / ACCENT_DISABLED
            GradientColor = 0x99000000u | ((uint)tint.B << 16) | ((uint)tint.G << 8) | tint.R,
        };
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<AccentPolicy>());
        try
        {
            Marshal.StructureToPtr(policy, pointer, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = 19, // WCA_ACCENT_POLICY
                Data = pointer,
                Size = (nuint)Marshal.SizeOf<AccentPolicy>(),
            };
            return SetWindowCompositionAttribute(hwnd, ref data);
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(nint hwnd, ref Margins margins);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        nint window,
        uint attribute,
        ref int value,
        uint size
    );

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        nint window,
        uint attribute,
        out int value,
        uint size
    );

    [DllImport("dwmapi.dll")]
    private static extern int DwmIsCompositionEnabled(
        [MarshalAs(UnmanagedType.Bool)] out bool enabled
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct PowerStatus
    {
        public byte ACLineStatus,
            BatteryFlag,
            BatteryLifePercent,
            SystemStatusFlag;
        public uint BatteryLifeTime,
            BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemPowerStatus(out PowerStatus status);

    [DllImport("user32.dll")]
    internal static extern nint RegisterPowerSettingNotification(
        nint recipient,
        ref Guid setting,
        uint flags
    );

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnregisterPowerSettingNotification(nint registration);

    internal static bool CanUseBackdrop(
        int build,
        bool transparency,
        bool highContrast,
        bool energySaver,
        bool composition
    ) => build >= 17134 && transparency && !highContrast && !energySaver && composition;

    private static bool BackdropEnabled()
    {
        // A remote-session flag does not imply composition is unavailable (for
        // example with GPU-backed remote desktops). Let DWM accept or reject it.
        try
        {
            var transparency =
                Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "EnableTransparency",
                    null
                )
                    is int value
                && value == 1;
            return CanUseBackdrop(
                Environment.OSVersion.Version.Build,
                transparency,
                SystemParameters.HighContrast,
                !GetSystemPowerStatus(out var power) || power.SystemStatusFlag != 0,
                DwmIsCompositionEnabled(out var composition) >= 0 && composition
            );
        }
        catch (Exception ex)
            when (ex
                    is System.IO.IOException
                        or UnauthorizedAccessException
                        or System.Security.SecurityException
            )
        {
            return false;
        }
    }

    internal static void WithoutPresentation(Window window, Action action)
    {
        var handle = new WindowInteropHelper(window).EnsureHandle();
        Marshal.ThrowExceptionForHR(
            DwmGetWindowAttribute(handle, 14, out var cloaked, sizeof(int))
        );
        // DWMWA_CLOAK hides the whole native surface while WPF still lays it out.
        // Preserve our existing cloak so nested positioning cannot reveal it early.
        var restore = cloaked & 1;
        var hidden = 1;
        Marshal.ThrowExceptionForHR(DwmSetWindowAttribute(handle, 13, ref hidden, sizeof(int)));
        try
        {
            action();
        }
        finally
        {
            Marshal.ThrowExceptionForHR(
                DwmSetWindowAttribute(handle, 13, ref restore, sizeof(int))
            );
        }
    }

    internal static void ApplyWindowsBackdrop(Window window, bool dark)
    {
        var fallback =
            SystemParameters.HighContrast ? SystemColors.WindowColor
            : window.TryFindResource("SolidBackgroundFillColorBase") is Color color ? color
            : dark ? Color.FromRgb(32, 32, 32)
            : Color.FromRgb(243, 243, 243);
        fallback.A = 255;
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == 0)
        {
            window.Background = new SolidColorBrush(fallback);
            return;
        }

        var corners = (int)DwmWindowCornerPreference.Round;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref corners, sizeof(int));
        var border = DWMWA_COLOR_DEFAULT;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref border, sizeof(int));
        var darkMode = dark ? 1 : 0;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

        var enabled = BackdropEnabled();
        var legacy = Environment.OSVersion.Version.Build < 22621;
        var backdrop = (int)(
            enabled ? DwmSystemBackdropType.TransientWindow : DwmSystemBackdropType.None
        );
        if (legacy)
        {
            enabled &= ApplyLegacyAcrylic(hwnd, enabled, fallback);
        }
        else
        {
            enabled &=
                DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int))
                >= 0;
        }
        var margins = enabled
            ? new Margins
            {
                Left = -1,
                Right = -1,
                Top = -1,
                Bottom = -1,
            }
            : default;
        enabled &= DwmExtendFrameIntoClientArea(hwnd, ref margins) >= 0;
        if (!enabled)
        {
            if (legacy)
            {
                _ = ApplyLegacyAcrylic(hwnd, false, fallback);
            }
            else
            {
                backdrop = (int)DwmSystemBackdropType.None;
                _ = DwmSetWindowAttribute(
                    hwnd,
                    DWMWA_SYSTEMBACKDROP_TYPE,
                    ref backdrop,
                    sizeof(int)
                );
            }
            margins = default;
            _ = DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        // Translucent child surfaces remain readable even when DWM cannot supply Acrylic.
        window.Background = enabled ? Brushes.Transparent : new SolidColorBrush(fallback);
        if (HwndSource.FromHwnd(hwnd) is HwndSource source)
        {
            source.CompositionTarget.BackgroundColor = enabled ? Colors.Transparent : fallback;
        }
    }
}
