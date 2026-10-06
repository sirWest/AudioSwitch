using AudioSwitch.Core.Settings;

namespace AudioSwitch.Core.Audio;

public sealed record AudioDevice(
    string Id,
    string Name,
    string Description,
    string IconPath,
    Direction Direction,
    bool Multimedia,
    bool Communications,
    bool Console
)
{
    public string DisplayName(AppSettings settings)
    {
        if (settings.ForDevice(Id) is { UseCustomName: true, CustomName.Length: > 0 } custom)
        {
            return custom.CustomName;
        }

        return settings.ShowHardwareName ? Name : Description;
    }

    public static AudioDevice[] SortByDisplayName(
        IEnumerable<AudioDevice> devices,
        AppSettings settings
    ) =>
        devices
            .OrderBy(d => d.DisplayName(settings), StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(d => d.Id, StringComparer.Ordinal)
            .ToArray();
}
