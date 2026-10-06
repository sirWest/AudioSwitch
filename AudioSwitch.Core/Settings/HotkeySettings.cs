namespace AudioSwitch.Core.Settings;

public sealed class HotkeySettings
{
    public HotkeyAction Function { get; set; }
    public uint Modifiers { get; set; }
    public int VirtualKey { get; set; }
    public bool ShowOsd { get; set; } = true;
}
