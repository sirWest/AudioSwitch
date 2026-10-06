using System.IO;
using Microsoft.Win32;

namespace AudioSwitch.Shell;

internal static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    internal static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue("AudioSwitch") is string
                || File.Exists(ShortcutPath("AudioSwitch"));
        }
    }

    internal static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
        {
            key.SetValue("AudioSwitch", $"\"{Environment.ProcessPath}\" --startup");
        }
        else
        {
            key.DeleteValue("AudioSwitch", false);
        }

        // The legacy installer used a shortcut; the app and installer now share one Run entry.
        File.Delete(ShortcutPath("AudioSwitch"));
    }

    private static string ShortcutPath(string name) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), name + ".lnk");
}
