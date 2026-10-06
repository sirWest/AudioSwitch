namespace AudioSwitch.Core.Settings;

public enum HotkeyMuteAction
{
    KeepCurrentState,
    Mute,
    Unmute,
}

public sealed class HotkeyDeviceSettings
{
    public string? DeviceId { get; set; }
    public string DeviceName { get; set; } = "";
    public HotkeyMuteAction MuteAction { get; set; }
    public bool MuteOthers { get; set; }

    public HotkeyDeviceSettings Clone() => (HotkeyDeviceSettings)MemberwiseClone();
}
