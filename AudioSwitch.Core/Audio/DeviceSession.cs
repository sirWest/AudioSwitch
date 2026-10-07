namespace AudioSwitch.Core.Audio;

// Managed observations only: taking a diagnostic snapshot never queries the driver.
internal sealed class DeviceSession(string? id = null, TimeProvider? timeProvider = null)
{
    private readonly object gate = new();
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;
    private readonly Dictionary<string, string> observations = new();
    private readonly Dictionary<string, (Exception Error, DateTimeOffset RetryAt)> failures = new();
    private readonly Dictionary<string, string?> properties = new();
    internal string? Id { get; } = id;
    internal int Generation { get; private set; }

    internal void Note(string key, string value)
    {
        lock (gate)
            observations[key] = value;
    }

    internal IReadOnlyDictionary<string, string> Snapshot()
    {
        lock (gate)
            return new Dictionary<string, string>(observations);
    }

    internal void Reset()
    {
        lock (gate)
        {
            failures.Clear();
            properties.Clear();
            observations.Clear();
            Generation++;
        }
    }

    internal bool TryProperty(string key, out string? value)
    {
        lock (gate)
            return properties.TryGetValue(key, out value);
    }

    internal void Property(string key, string? value)
    {
        bool recovered;
        lock (gate)
        {
            properties[key] = value;
            observations[key] = value ?? "<not supplied>";
            recovered = failures.Remove(key);
        }
        if (recovered)
            AudioDiagnostics.Log.Recovered(key, Id, Snapshot);
    }

    internal Exception? Blocked(string key)
    {
        lock (gate)
            return failures.TryGetValue(key, out var failure) && clock.GetUtcNow() < failure.RetryAt
                ? failure.Error
                : null;
    }

    internal void Failed(string key, Exception error, TimeSpan? retryDelay = null)
    {
        lock (gate)
        {
            // Only explicit unsupported results are permanent for this device generation.
            var unsupported =
                error.HResult
                is unchecked((int)0x80004002)
                    or unchecked((int)0x80004001)
                    or unchecked((int)0x80070032);
            var delay = AudioUnavailableException.IsUnavailable(error) ? 2 : 30;
            failures[key] = (
                error,
                unsupported
                    ? DateTimeOffset.MaxValue
                    : clock.GetUtcNow() + (retryDelay ?? TimeSpan.FromSeconds(delay))
            );
            observations[key] =
                $"{(unsupported ? "unsupported" : "failed")}: 0x{error.HResult:X8}: {error.Message}";
        }
        AudioDiagnostics.Log.Failure(key, error, Id, Snapshot);
    }

    internal void Succeeded(string key)
    {
        bool recovered;
        lock (gate)
        {
            recovered = failures.Remove(key);
            if (recovered)
                observations[key] = "recovered";
            else
                observations[key] = "supported";
        }
        if (recovered)
            AudioDiagnostics.Log.Recovered(key, Id, Snapshot);
    }
}
