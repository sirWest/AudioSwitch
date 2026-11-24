namespace AudioSwitch.Core.Settings;

public enum HotkeyAction
{
    PreviousPlaybackDevice,
    NextPlaybackDevice,
    PreviousRecordingDevice,
    NextRecordingDevice,
    TogglePlaybackMute,
    ToggleRecordingMute,
    PlaybackVolumeUp,
    PlaybackVolumeDown,
    RecordingVolumeUp,
    RecordingVolumeDown,
    SelectAudioDevices,
}

public static class HotkeyActionExtensions
{
    public static bool IsVolumeAdjustment(this HotkeyAction action) =>
        action is HotkeyAction.PlaybackVolumeUp or HotkeyAction.PlaybackVolumeDown
            or HotkeyAction.RecordingVolumeUp or HotkeyAction.RecordingVolumeDown;
}
