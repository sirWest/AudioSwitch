using System.Diagnostics;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Core.Audio;

public sealed class DeviceHotkeyExecutor
{
    private readonly Func<Direction, IReadOnlyList<AudioDevice>> list;
    private readonly Action<AudioDevice, bool> select;
    private readonly Action<string, bool> mute;

    public DeviceHotkeyExecutor(AudioService audio)
        : this(
            audio.List,
            (device, communications) =>
                audio.SetDefault(
                    device.Id,
                    communications
                        ? [AudioRole.Multimedia, AudioRole.Communications]
                        : [AudioRole.Multimedia]
                ),
            (id, value) => audio.SetMute(id, value)
        ) { }

    internal DeviceHotkeyExecutor(
        Func<Direction, IReadOnlyList<AudioDevice>> list,
        Action<AudioDevice, bool> select,
        Action<string, bool> mute
    )
    {
        this.list = list;
        this.select = select;
        this.mute = mute;
    }

    public IReadOnlyList<string> Execute(HotkeySettings hotkey, AppSettings settings)
    {
        var completed = new List<string>();
        Action? unmuteRecording = null;
        // Finish requested microphone muting before changing playback routing.
        Apply(Direction.Recording, hotkey.Recording);
        Apply(Direction.Playback, hotkey.Playback);
        // Resume the microphone only after playback has had a chance to switch.
        unmuteRecording?.Invoke();
        return completed;

        void Apply(Direction direction, HotkeyDeviceSettings options)
        {
            if (options.DeviceId is null && !options.MuteOthers)
            {
                return;
            }

            IReadOnlyList<AudioDevice> devices = [];
            if (!Try(() => devices = list(direction)))
            {
                return;
            }

            var target = devices.FirstOrDefault(d => d.Id == options.DeviceId);
            // A missing explicit target must not turn "mute others" into "mute all".
            if (options.DeviceId is not null && target is null)
            {
                return;
            }

            bool CanMute(AudioDevice device) =>
                settings.ForDevice(device.Id)?.ExcludeFromHotkeyMute != true;

            if (
                target is not null
                && options.MuteAction == HotkeyMuteAction.Mute
                && CanMute(target)
            )
            {
                Try(() => mute(target.Id, true));
            }

            if (options.MuteOthers)
            {
                var count = 0;
                foreach (var other in devices.Where(d => d.Id != target?.Id && CanMute(d)))
                {
                    if (Try(() => mute(other.Id, true)))
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    completed.Add(
                        $"{count} {direction.ToString().ToLowerInvariant()} device(s) muted"
                    );
                }
            }

            if (
                target is null
                || !Try(() =>
                    select(target, hotkey.AlsoCommunications ?? settings.AlsoCommunications)
                )
            )
            {
                return;
            }

            var label = target.DisplayName(settings);
            var feedbackIndex = completed.Count;
            completed.Add(label);
            if (options.MuteAction == HotkeyMuteAction.Unmute && CanMute(target))
            {
                void Unmute()
                {
                    if (Try(() => mute(target.Id, false)))
                    {
                        completed[feedbackIndex] = label + " (unmuted)";
                    }
                }
                if (direction == Direction.Recording)
                {
                    unmuteRecording = Unmute;
                }
                else
                {
                    Unmute();
                }
            }
        }
    }

    private static bool Try(Action action)
    {
        try
        {
            action();
            return true;
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            Trace.TraceWarning(
                $"Device hotkey skipped an unavailable audio operation: {ex.Message}"
            );
            return false;
        }
    }
}
