using AudioSwitch.Core.CoreAudio;

namespace AudioSwitch.Core.Audio;

public sealed class EndpointMonitor : IDisposable
{
    private readonly MMDevice device;
    private readonly PeakReader peaks;
    private bool disposed;
    public string Id { get; }

    public event Action<AudioState>? Changed;

    internal EndpointMonitor(MMDevice device)
    {
        this.device = device;
        peaks = new(() => device.AudioMeterInformation.PeakValues, device.ResetMeter);
        try
        {
            Id = device.ID;
            if (device.State != DeviceState.Active)
            {
                throw new AudioUnavailableException(
                    "The selected audio device is no longer active."
                );
            }

            device.AudioEndpointVolume.Subscribe(state => Changed?.Invoke(state));
        }
        catch
        {
            device.Dispose();
            throw;
        }
    }

    public AudioState State =>
        new(device.AudioEndpointVolume.MasterVolumeLevelScalar, device.AudioEndpointVolume.Mute);

    public (float Left, float Right) Peaks()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return peaks.Read();
    }

    public void Dispose()
    {
        disposed = true;
        device.Dispose();
        Changed = null;
    }
}
