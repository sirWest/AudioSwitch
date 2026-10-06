using AudioSwitch.Core.Settings;

namespace AudioSwitch.Settings;

internal static class HotkeyPresentation
{
    public static string ActionName(HotkeyAction action) =>
        action switch
        {
            HotkeyAction.SelectAudioDevices => "Select audio devices",
            HotkeyAction.PreviousPlaybackDevice => "Previous playback device",
            HotkeyAction.NextPlaybackDevice => "Next playback device",
            HotkeyAction.PreviousRecordingDevice => "Previous recording device",
            HotkeyAction.NextRecordingDevice => "Next recording device",
            HotkeyAction.TogglePlaybackMute => "Toggle playback mute",
            HotkeyAction.ToggleRecordingMute => "Toggle recording mute",
            HotkeyAction.PlaybackVolumeUp => "Playback volume up",
            HotkeyAction.PlaybackVolumeDown => "Playback volume down",
            HotkeyAction.RecordingVolumeUp => "Recording volume up",
            HotkeyAction.RecordingVolumeDown => "Recording volume down",
            _ => action.ToString(),
        };

    public static string MuteName(HotkeyMuteAction action) =>
        action switch
        {
            HotkeyMuteAction.Mute => "Mute",
            HotkeyMuteAction.Unmute => "Unmute",
            _ => "Keep current mute state",
        };

    public static string DeviceName(HotkeyDeviceSettings options, AppSettings settings)
    {
        var preference = settings.ForDevice(options.DeviceId ?? "");
        return preference is { UseCustomName: true, CustomName.Length: > 0 } ? preference.CustomName
            : string.IsNullOrWhiteSpace(options.DeviceName) ? options.DeviceId ?? "Leave unchanged"
            : options.DeviceName;
    }

    public static string Describe(HotkeySettings hotkey, AppSettings settings) =>
        $"Playback: {DescribeDevice(hotkey.Playback, settings)}\nRecording: {DescribeDevice(hotkey.Recording, settings)}";

    private static string DescribeDevice(HotkeyDeviceSettings options, AppSettings settings)
    {
        if (options.DeviceId is null)
        {
            return options.MuteOthers ? "unchanged; mute all eligible devices" : "unchanged";
        }
        var description = DeviceName(options, settings);
        description +=
            settings.ForDevice(options.DeviceId)?.ExcludeFromHotkeyMute == true
                ? "; excluded from muting"
                : $"; {MuteName(options.MuteAction).ToLowerInvariant()}";
        return options.MuteOthers ? description + "; mute other eligible devices" : description;
    }
}
