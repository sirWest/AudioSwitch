using System.IO;
using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;
using Microsoft.Win32;

namespace AudioSwitch.Shell;

internal static partial class TrayVisibility
{
    [LibraryImport("shell32.dll")]
    private static partial int SHGetKnownFolderPath(in Guid folderId, uint flags, nint token, out nint path);

    internal static async Task<bool> PromoteAsync()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621))
        {
            return true;
        }

        try
        {
            using var user = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            // Explorer creates this entry only after Shell_NotifyIcon registers the icon.
            for (var attempt = 0; attempt <= 40; attempt++)
            {
                using var root = user.OpenSubKey(@"Control Panel\NotifyIconSettings", writable: true);
                if (root is not null && Promote(root, Environment.ProcessPath!))
                {
                    return true;
                }

                if (attempt < 40)
                {
                    await Task.Delay(250);
                }
            }
            throw new TimeoutException("Explorer did not expose a matching tray entry within 10 seconds.");
        }
        catch (Exception ex)
        {
            AudioDiagnostics.Log.Failure("configure tray visibility", ex);
            return false;
        }
    }

    internal static bool Promote(RegistryKey root, string executablePath)
    {
        var matched = false;
        foreach (var name in root.GetSubKeyNames())
        {
            using var entry = root.OpenSubKey(name, writable: true);
            if (entry?.GetValue("ExecutablePath") is not string path ||
                !string.Equals(ResolvePath(path), Path.GetFullPath(executablePath),
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // This undocumented Windows 11 preference is changed only on explicit request.
            entry.SetValue("IsPromoted", 1, RegistryValueKind.DWord);
            matched = true;
        }
        return matched;
    }

    internal static string? ResolvePath(string path)
    {
        try
        {
            path = Environment.ExpandEnvironmentVariables(path);
            if (path.StartsWith('{'))
            {
                if (path.Length < 39 || path[38] != '\\' || !Guid.TryParse(path[..38], out var folderId))
                {
                    return null;
                }

                nint buffer = 0;
                try
                {
                    if (SHGetKnownFolderPath(in folderId, 0, 0, out buffer) != 0)
                    {
                        return null;
                    }

                    path = Marshal.PtrToStringUni(buffer) + path[38..];
                }
                finally
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }
            return Path.IsPathFullyQualified(path) ? Path.GetFullPath(path) : null;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
