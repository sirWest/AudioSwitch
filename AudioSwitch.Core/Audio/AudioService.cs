using System.Runtime.InteropServices;
using AudioSwitch.Core.CoreAudio;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Core.Audio;

public sealed class AudioService : IDisposable
{
    private MMDeviceEnumerator enumerator;
    private readonly EndpointNotifications notifications;
    private bool notificationsRegistered;
    public bool NotificationsAvailable => notificationsRegistered;
    public event Action<AudioChange>? Changed;

    public AudioService()
        : this(new MMDeviceEnumerator()) { }

    internal AudioService(MMDeviceEnumerator enumerator)
    {
        this.enumerator = enumerator;
        notifications = new(change => Changed?.Invoke(change));
        EnsureNotifications();
    }

    private void EnsureNotifications()
    {
        if (notificationsRegistered)
        {
            return;
        }

        try
        {
            enumerator.RegisterEndpointNotificationCallback(notifications);
            notificationsRegistered = true;
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            System.Diagnostics.Trace.TraceWarning($"Audio notifications unavailable: {ex.Message}");
            enumerator.Dispose();
            enumerator = new();
        }
    }

    private static DataFlow Flow(Direction direction) =>
        direction == Direction.Playback ? DataFlow.Render : DataFlow.Capture;

    public string? DefaultId(Direction direction, AudioRole role = AudioRole.Multimedia)
    {
        try
        {
            using var device = enumerator.GetDefaultAudioEndpoint(Flow(direction), (Role)role);
            return device.ID;
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            if (!AudioUnavailableException.IsUnavailable(ex))
            {
                System.Diagnostics.Trace.TraceWarning(
                    $"Default {direction}/{role} unavailable: {ex.Message}"
                );
            }
            return null;
        }
    }

    public IReadOnlyList<AudioDevice> List(Direction direction)
    {
        EnsureNotifications();
        var multimedia = DefaultId(direction);
        var communications = DefaultId(direction, AudioRole.Communications);
        var console = DefaultId(direction, AudioRole.Console);
        var result = new List<AudioDevice>();
        try
        {
            foreach (
                var device in enumerator.EnumerateAudioEndPoints(
                    Flow(direction),
                    DeviceState.Active
                )
            )
            {
                using (device)
                {
                    try
                    {
                        if (device.State != DeviceState.Active)
                        {
                            continue;
                        }

                        var id = device.ID;
                        if (string.IsNullOrWhiteSpace(id))
                        {
                            continue;
                        }
                        result.Add(
                            new(
                                id,
                                device.FriendlyName,
                                device.DeviceFriendlyName,
                                device.IconPath,
                                direction,
                                id == multimedia,
                                id == communications,
                                id == console
                            )
                        );
                    }
                    catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
                    {
                        // A collection snapshot can outlive one of its endpoints.
                        System.Diagnostics.Trace.TraceWarning(
                            $"Skipping unavailable audio endpoint: {ex.Message}"
                        );
                    }
                }
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            // A disconnected audio service can leave the enumerator's COM proxy stale.
            enumerator.Dispose();
            enumerator = new();
            notificationsRegistered = false;
            throw;
        }

        return result
            .OrderBy(d => d.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(d => d.Id, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<AudioDevice> List(Direction direction, AppSettings settings) =>
        AudioDevice.SortByDisplayName(List(direction), settings);

    public AudioDevice Resolve(Direction direction, string selector, AppSettings settings)
    {
        var all = List(direction);
        var byId = all.FirstOrDefault(d =>
            d.Id.Equals(selector, StringComparison.OrdinalIgnoreCase)
        );
        if (byId is not null)
        {
            return byId;
        }

        var matches = all.Where(d =>
                d.Name.Equals(selector, StringComparison.OrdinalIgnoreCase)
                || d.DisplayName(settings).Equals(selector, StringComparison.OrdinalIgnoreCase)
            )
            .ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new ArgumentException(
                "No matching active device. Use list to obtain its ID."
            ),
            _ => throw new ArgumentException("Device name is ambiguous. Use its ID."),
        };
    }

    public void SetDefault(string id, params AudioRole[] roles)
    {
        using var device = enumerator.GetDevice(id);
        if (device.State != DeviceState.Active)
        {
            throw new AudioUnavailableException("The selected audio device is no longer active.");
        }

        var instance = new PolicyConfigObject();
        try
        {
            var policy = (IPolicyConfig)instance;
            foreach (var role in roles.Distinct())
            {
                var result = policy.SetDefaultEndpoint(id, (Role)role);
                AudioOperationException.Check(result, $"set the {role} default device '{id}'");
            }
        }
        finally
        {
            Marshal.ReleaseComObject(instance);
        }
    }

    public void Select(AudioDevice device, AppSettings settings)
    {
        using var endpoint = enumerator.GetDevice(device.Id);
        if (endpoint.State != DeviceState.Active)
        {
            throw new AudioUnavailableException("The selected audio device is no longer active.");
        }

        var roles = new List<AudioRole>();
        if (DefaultId(device.Direction) != device.Id)
        {
            roles.Add(AudioRole.Multimedia);
        }

        if (
            settings.AlsoCommunications
            && DefaultId(device.Direction, AudioRole.Communications) != device.Id
        )
        {
            roles.Add(AudioRole.Communications);
        }

        if (roles.Count > 0)
        {
            SetDefault(device.Id, roles.ToArray());
        }
    }

    public AudioDevice Cycle(Direction direction, bool previous, AppSettings settings)
    {
        var devices = List(direction, settings)
            .Where(d => settings.ForDevice(d.Id)?.Hidden != true)
            .ToArray();
        if (devices.Length == 0)
        {
            throw new AudioUnavailableException("No visible active audio devices.");
        }

        var currentIndex = Array.FindIndex(devices, d => d.Multimedia);
        int nextIndex;
        if (currentIndex < 0)
        {
            // A hidden or missing default is outside the cycle; start at the requested end.
            nextIndex = previous ? devices.Length - 1 : 0;
        }
        else
        {
            var step = previous ? -1 : 1;
            nextIndex = (currentIndex + step + devices.Length) % devices.Length;
        }

        var nextDevice = devices[nextIndex];
        Select(nextDevice, settings);
        return nextDevice;
    }

    public AudioState State(string id)
    {
        using var device = enumerator.GetDevice(id);
        return new(
            device.AudioEndpointVolume.MasterVolumeLevelScalar,
            device.AudioEndpointVolume.Mute
        );
    }

    public AudioState SetVolume(string id, float volume)
    {
        if (!float.IsFinite(volume))
        {
            throw new ArgumentOutOfRangeException(nameof(volume));
        }

        using var device = enumerator.GetDevice(id);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(volume, 0, 1);
        return new(
            device.AudioEndpointVolume.MasterVolumeLevelScalar,
            device.AudioEndpointVolume.Mute
        );
    }

    public AudioState SetMute(string id, bool? mute = null)
    {
        using var device = enumerator.GetDevice(id);
        device.AudioEndpointVolume.Mute = mute ?? !device.AudioEndpointVolume.Mute;
        return new(
            device.AudioEndpointVolume.MasterVolumeLevelScalar,
            device.AudioEndpointVolume.Mute
        );
    }

    public EndpointMonitor Monitor(string id) => new(enumerator.GetDevice(id));

    public void ApplyStartup(AppSettings settings)
    {
        var failures = new List<Exception>();
        foreach (var direction in Enum.GetValues<Direction>())
        {
            foreach (var device in List(direction))
            {
                var pref = settings.ForDevice(device.Id);
                foreach (var role in new[] { AudioRole.Multimedia, AudioRole.Communications })
                {
                    var apply =
                        role == AudioRole.Multimedia
                            ? pref?.StartupMultimedia == true && !device.Multimedia
                            : pref?.StartupCommunications == true && !device.Communications;
                    if (!apply)
                    {
                        continue;
                    }

                    try
                    {
                        SetDefault(device.Id, role);
                    }
                    catch (Exception ex)
                    {
                        failures.Add(
                            new InvalidOperationException(
                                $"{device.Name} ({role}): {ex.Message}",
                                ex
                            )
                        );
                    }
                }
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException(
                "Some startup audio preferences could not be applied.",
                failures
            );
        }
    }

    public void Dispose()
    {
        notifications.Stop();
        enumerator.Dispose();
    }
}
