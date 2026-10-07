using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Core.Audio;

public static class AudioDiagnostics
{
    public static string LogPath { get; } =
        Path.Combine(Path.GetDirectoryName(SettingsStore.DefaultPath)!, "errors.log");

    public static AudioDiagnosticLog Log { get; } = new(LogPath);
}

public sealed class AudioDiagnosticLog
{
    private readonly string path;
    private readonly TimeProvider clock;
    private readonly long maxBytes;
    private readonly object gate = new();
    private readonly Dictionary<string, (DateTimeOffset At, int Suppressed)> recent = new();

    [ThreadStatic]
    private static bool writing;

    public AudioDiagnosticLog(string path)
        : this(path, TimeProvider.System, 1024 * 1024) { }

    internal AudioDiagnosticLog(string path, TimeProvider clock, long maxBytes)
    {
        this.path = path;
        this.clock = clock;
        this.maxBytes = maxBytes;
    }

    // Diagnostic enrichment and disk failures must never replace the original failure.
    public void Failure(
        string operation,
        Exception error,
        string? deviceId = null,
        Func<IReadOnlyDictionary<string, string>>? details = null
    ) => Write(operation, error, deviceId, details);

    public void Recovered(
        string operation,
        string? deviceId = null,
        Func<IReadOnlyDictionary<string, string>>? details = null
    ) => Write(operation, null, deviceId, details);

    private void Write(
        string operation,
        Exception? error,
        string? deviceId,
        Func<IReadOnlyDictionary<string, string>>? details
    )
    {
        if (writing)
            return;
        writing = true;
        try
        {
            lock (gate)
            {
                var now = clock.GetUtcNow();
                var key = $"{operation}|{deviceId}|{error?.HResult:X8}";
                recent.TryGetValue(key, out var previous);
                if (now - previous.At < TimeSpan.FromMinutes(1))
                {
                    recent[key] = (previous.At, previous.Suppressed + 1);
                    return;
                }
                if (recent.Count >= 512)
                    recent.Clear();
                recent[key] = (now, 0);
                var data = new Dictionary<string, string>
                {
                    ["timestamp"] = now.ToString("O"),
                    ["operation"] = operation,
                    ["event"] = error is null ? "recovered" : "failure",
                    ["hresult"] = error is null ? "" : $"0x{error.HResult:X8}",
                    ["deviceId"] = deviceId ?? "<not available>",
                    ["deviceInfo"] =
                        "Cached observations; unlisted fields have not been read. No device queries during logging.",
                    ["suppressedSincePrevious"] = previous.Suppressed.ToString(),
                };
                if (error is not null)
                    Add("exception", () => error.ToString());
                Add("os", () => RuntimeInformation.OSDescription);
                Add("osVersion", () => Environment.OSVersion.VersionString);
                Add("osArchitecture", () => RuntimeInformation.OSArchitecture.ToString());
                Add("processArchitecture", () => RuntimeInformation.ProcessArchitecture.ToString());
                Add("runtime", () => RuntimeInformation.FrameworkDescription);
                Add(
                    "appVersion",
                    () =>
                        typeof(AudioDiagnostics)
                            .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                            ?.InformationalVersion
                        ?? "unknown"
                );
                Add("buildId", () => typeof(AudioDiagnostics).Module.ModuleVersionId.ToString());
                Add("process", () => Environment.ProcessId.ToString());
                Add(
                    "session",
                    () =>
                    {
                        using var p = Process.GetCurrentProcess();
                        return p.SessionId.ToString();
                    }
                );
                Add(
                    "thread",
                    () =>
                        $"{Environment.CurrentManagedThreadId} / {Thread.CurrentThread.GetApartmentState()}"
                );
                Add("interactive", () => Environment.UserInteractive.ToString());
                if (details is not null)
                {
                    try
                    {
                        foreach (var pair in details().Take(64))
                            data["device." + pair.Key] = Limit(pair.Value, 4096);
                    }
                    catch (Exception ex)
                    {
                        data["diagnosticCollectionError"] = $"0x{ex.HResult:X8}: {ex.Message}";
                    }
                }
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
                if (File.Exists(path) && new FileInfo(path).Length >= maxBytes)
                    File.Move(path, path + ".1", true);
                File.AppendAllText(path, JsonSerializer.Serialize(data) + Environment.NewLine);

                void Add(string name, Func<string> read)
                {
                    try
                    {
                        data[name] = Limit(read(), 16384);
                    }
                    catch (Exception ex)
                    {
                        data[name] = $"<unavailable: 0x{ex.HResult:X8}>";
                    }
                }
            }
        }
        catch (Exception)
        { /* Logging is best effort, including access and serialization errors. */
        }
        finally
        {
            writing = false;
        }
    }

    private static string Limit(string value, int length) =>
        value.Length <= length ? value : value[..length] + " <truncated>";
}
