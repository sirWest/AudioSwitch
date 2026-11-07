namespace AudioSwitch.Core.Audio;

internal sealed class PeakReader(
    Func<IReadOnlyList<float>> read,
    Action reset,
    TimeProvider? timeProvider = null,
    DeviceSession? session = null
)
{
    private readonly DeviceSession session = session ?? new(timeProvider: timeProvider);
    private int generation = session?.Generation ?? 0;
    private bool failed;

    internal (float Left, float Right) Read()
    {
        if (generation != session.Generation)
        {
            generation = session.Generation;
            reset();
        }
        if (session.Blocked("read peak levels") is not null)
            return (0, 0);

        try
        {
            var channels = read();
            if (failed)
                session.Succeeded("read peak levels");
            failed = false;
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
            session.Failed("read peak levels", ex, TimeSpan.FromSeconds(5));
            failed = true;
            reset();
            return (0, 0);
        }
    }
}
