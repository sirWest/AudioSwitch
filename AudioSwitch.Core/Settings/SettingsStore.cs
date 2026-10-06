using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioSwitch.Core.Settings;

public sealed class SettingsStore(string? path = null)
{
    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };
    public static string DefaultPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AudioSwitch",
            "settings.json"
        );
    public string FilePath { get; } = path ?? DefaultPath;
    public string? RecoveryNotice { get; private set; }

    private string? loadedText;

    public AppSettings Load()
    {
        RecoveryNotice = null;
        if (!File.Exists(FilePath))
        {
            loadedText = null;
            return new();
        }

        loadedText = File.ReadAllText(FilePath);
        try
        {
            return Parse(loadedText);
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException)
        {
            // Never silently overwrite unreadable or future-version settings.
            if (ex is InvalidDataException && loadedText.Contains("\"Version\""))
            {
                using var doc = JsonDocument.Parse(loadedText);
                if (
                    doc.RootElement.TryGetProperty("Version", out var version)
                    && version.GetInt32() != 1
                )
                {
                    throw;
                }
            }

            if (!File.Exists(FilePath + ".bak"))
            {
                throw new InvalidDataException(
                    $"Cannot read {FilePath}. Your settings have been preserved.",
                    ex
                );
            }

            var restored = Parse(File.ReadAllText(FilePath + ".bak"));
            RecoveryNotice =
                "Settings were recovered from the backup. The damaged file will be preserved when you save.";
            return restored;
        }
    }

    private static AppSettings Parse(string text)
    {
        var settings =
            JsonSerializer.Deserialize<AppSettings>(text, Json)
            ?? throw new InvalidDataException("Empty settings.");
        settings.Validate();
        return settings;
    }

    public void Save(AppSettings settings)
    {
        settings.Validate();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
        var normalizedPath = Path.GetFullPath(FilePath).ToUpperInvariant();
        var pathBytes = System.Text.Encoding.UTF8.GetBytes(normalizedPath);
        var mutexKey = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pathBytes));
        using var gate = new Mutex(false, "Local\\AudioSwitch.Settings." + mutexKey);
        try
        {
            if (!gate.WaitOne(TimeSpan.FromSeconds(5)))
            {
                throw new IOException("Settings are busy. Try again.");
            }
        }
        catch (AbandonedMutexException)
        {
            // The previous writer exited without releasing the mutex; this thread now owns it.
        }

        var temp = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var current = File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
            // Compare inside the mutex so two editors cannot both replace the same snapshot.
            if (current != loadedText)
            {
                throw new IOException(
                    "Settings changed in another process. Reopen settings before saving."
                );
            }

            // Prune the saved snapshot without changing objects held by an open device editor.
            var snapshot = settings.Clone();
            snapshot.Devices.RemoveAll(device => !device.HasCustomSettings);
            var text = JsonSerializer.Serialize(snapshot, Json);
            // Flush the complete replacement before atomically swapping it into place.
            using (
                var stream = new FileStream(
                    temp,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough
                )
            )
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                stream.Write(bytes);
                stream.Flush(true);
            }

            if (current is not null)
            {
                if (RecoveryNotice is not null)
                {
                    File.Copy(
                        FilePath,
                        FilePath + ".damaged-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
                    );
                }

                File.Replace(temp, FilePath, RecoveryNotice is null ? FilePath + ".bak" : null);
            }
            else
            {
                File.Move(temp, FilePath);
            }

            loadedText = text;
            RecoveryNotice = null;
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }

            gate.ReleaseMutex();
        }
    }
}
