using System.Security.Cryptography;
using System.Text.Json;

namespace AudioSwitch.Core.Settings;

public sealed class DeviceIconStore(string settingsPath)
{
    public string DirectoryPath { get; } =
        Path.Combine(Path.GetDirectoryName(Path.GetFullPath(settingsPath))!, "DeviceIcons");

    public static bool IsValidName(string? name) =>
        name is not null
        && (
            name.Length == 0
            || name.Length == 68
                && name.EndsWith(".png", StringComparison.Ordinal)
                && name.AsSpan(0, 64).ContainsAnyExcept("0123456789abcdef".AsSpan()) == false
        );

    public static string Name(byte[] png) =>
        Convert.ToHexStringLower(SHA256.HashData(png)) + ".png";

    public string Resolve(string name)
    {
        if (string.IsNullOrEmpty(name) || !IsValidName(name))
            throw new ArgumentException("Invalid device icon name.", nameof(name));
        return Path.Combine(DirectoryPath, name);
    }

    public void Save(
        SettingsStore store,
        AppSettings settings,
        IReadOnlyDictionary<string, byte[]> pending
    )
    {
        settings.Validate();
        var key = Convert.ToHexString(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(DirectoryPath.ToUpperInvariant()))
        );
        using var gate = new Mutex(false, "Local\\AudioSwitch.DeviceIcons." + key);
        try
        {
            if (!gate.WaitOne(TimeSpan.FromSeconds(5)))
                throw new IOException("Device icons are busy. Try again.");
        }
        catch (AbandonedMutexException) { }
        try
        {
            foreach (
                var name in settings
                    .Devices.Select(d => d.CustomIcon)
                    .Where(n => n.Length > 0)
                    .Distinct()
            )
            {
                if (!pending.TryGetValue(name, out var bytes))
                    continue;
                if (Name(bytes) != name)
                    throw new InvalidDataException("Device icon content does not match its name.");
                Directory.CreateDirectory(DirectoryPath);
                var path = Resolve(name);
                if (File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))
                    continue;
                var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    File.WriteAllBytes(temp, bytes);
                    File.Move(temp, path, true);
                }
                finally
                {
                    if (File.Exists(temp))
                        File.Delete(temp);
                }
            }
            store.Save(settings);
            Prune(store.FilePath);
        }
        finally
        {
            gate.ReleaseMutex();
        }
    }

    private void Prune(string currentPath)
    {
        if (!Directory.Exists(DirectoryPath))
            return;
        try
        {
            // Preserve other profiles, disconnected devices, backups and recoverable snapshots.
            // Unreadable snapshots defer cleanup rather than risk losing their images.
            var profiles = Directory
                .EnumerateFiles(Path.GetDirectoryName(DirectoryPath)!)
                .Concat(new[] { currentPath, currentPath + ".bak" })
                .Where(File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase);
            var retained = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in profiles)
            {
                if (
                    !Path.GetFileName(path).Contains(".json", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(path, currentPath, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(
                        path,
                        currentPath + ".bak",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    using var reader = File.OpenText(path);
                    int first;
                    do
                    {
                        first = reader.Read();
                    } while (first >= 0 && char.IsWhiteSpace((char)first));
                    if (first != '{')
                        continue;
                }
                var saved = JsonSerializer.Deserialize<AppSettings>(
                    File.ReadAllText(path),
                    SettingsStore.Json
                );
                if (saved is null)
                    return;
                saved.Validate();
                retained.UnionWith(saved.Devices.Select(d => d.CustomIcon));
            }
            foreach (var path in Directory.EnumerateFiles(DirectoryPath, "*.png"))
                if (
                    IsValidName(Path.GetFileName(path))
                    && !retained.Contains(Path.GetFileName(path))
                )
                    File.Delete(path);
        }
        catch (Exception ex)
            when (ex
                    is IOException
                        or UnauthorizedAccessException
                        or JsonException
                        or InvalidDataException
            )
        {
            System.Diagnostics.Trace.TraceWarning($"Device icon cleanup deferred: {ex.Message}");
        }
    }
}
