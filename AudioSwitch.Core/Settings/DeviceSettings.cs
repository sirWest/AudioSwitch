using AudioSwitch.Core.Audio;

namespace AudioSwitch.Core.Settings;

public sealed class DeviceSettings
{
    public string Id { get; set; } = "";
    public Direction Direction { get; set; }
    public bool Hidden { get; set; }
    public bool ExcludeFromHotkeyMute { get; set; }
    public bool UseCustomName { get; set; }
    public string CustomName { get; set; } = "";
    public int Hue { get; set; }
    public int Saturation { get; set; }
    public int Brightness { get; set; }
    public bool StartupMultimedia { get; set; }
    public bool StartupCommunications { get; set; }
}
