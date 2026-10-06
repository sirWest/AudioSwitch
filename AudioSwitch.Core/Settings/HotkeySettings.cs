namespace AudioSwitch.Core.Settings;

public sealed class HotkeySettings
{
    public HotkeyAction Function { get; set; }
    public uint Modifiers { get; set; }
    public int VirtualKey { get; set; }
    public bool ShowOsd { get; set; } = true;
    public string Name { get; set; } = "";
    public HotkeyDeviceSettings Playback { get; set; } = new();
    public HotkeyDeviceSettings Recording { get; set; } = new();
    public bool? AlsoCommunications { get; set; }

    public HotkeySettings Clone() =>
        new()
        {
            Function = Function,
            Modifiers = Modifiers,
            VirtualKey = VirtualKey,
            ShowOsd = ShowOsd,
            Name = Name,
            Playback = Playback.Clone(),
            Recording = Recording.Clone(),
            AlsoCommunications = AlsoCommunications,
        };
}
