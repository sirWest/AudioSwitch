using System.Diagnostics;

namespace AudioSwitch.Core.Audio;

internal sealed class PeakReader(
    Func<IReadOnlyList<float>> read,
    Action reset,
    TimeProvider? timeProvider = null
)
{
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;
    private DateTimeOffset retryAt;
    private bool failed;

    internal (float Left, float Right) Read()
    {
        if (clock.GetUtcNow() < retryAt)
        {
            return (0, 0);
        }

        try
        {
            var channels = read();
            failed = false;
            retryAt = default;
            // Mono endpoints drive both meters; additional channels are not displayed.
            return channels.Count switch
            {
                0 => (0, 0),
                1 => (channels[0], channels[0]),
                _ => (channels[0], channels[1]),
            };
        }
        catch (Exception ex) when (AudioUnavailableException.IsUnavailable(ex))
        {
            // An invalidated endpoint needs a new volume subscription as well.
            throw;
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            if (!failed)
            {
                Trace.TraceWarning($"Audio metering unavailable: {ex.Message}");
            }

            failed = true;
            retryAt = clock.GetUtcNow() + TimeSpan.FromSeconds(5);
            reset();
            return (0, 0);
        }
    }
}
