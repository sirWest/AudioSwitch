using System.Text.Json;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Core.Settings;

public sealed class AppSettings
{
    // Settings schema version, independent of the application release number.
    public int Version { get; set; } = 1;
    public AppTheme Theme { get; set; } = AppTheme.System;
    public Direction DefaultDirection { get; set; }
    public bool ShowBothDeviceGroups { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public DeviceGroup DefaultDeviceGroup
    {
        get =>
            ShowBothDeviceGroups ? DeviceGroup.Both
            : DefaultDirection == Direction.Recording ? DeviceGroup.Recording
            : DeviceGroup.Playback;
        set
        {
            ShowBothDeviceGroups = value == DeviceGroup.Both;
            DefaultDirection =
                value == DeviceGroup.Recording ? Direction.Recording : Direction.Playback;
        }
    }
    public bool AlsoCommunications { get; set; }
    public bool ColorVu { get; set; }
    public bool ShowHardwareName { get; set; } = true;
    public bool QuickSwitch { get; set; }
    public bool QuickSwitchOsd { get; set; } = true;
    public bool CloseAfterSelecting { get; set; }
    public bool AlwaysVisible { get; set; }
    public double? FlyoutLeft { get; set; }
    public double? FlyoutTop { get; set; }
    public bool CustomOsd { get; set; } = true;
    public bool VolumeScroll { get; set; }
    public ScrollKeys ScrollKeys { get; set; } = ScrollKeys.LWin;
    public bool ScrollOsd { get; set; } = true;
    public OsdSettings Osd { get; set; } = new();
    public List<DeviceSettings> Devices { get; set; } = [];
    public List<HotkeySettings> Hotkeys { get; set; } = [];

    public DeviceSettings? ForDevice(string id) => Devices.Find(d => d.Id == id);

    public AppSettings Clone() =>
        JsonSerializer.Deserialize<AppSettings>(
            JsonSerializer.Serialize(this, SettingsStore.Json),
            SettingsStore.Json
        )!;

    public void Validate()
    {
        if (Version != 1)
        {
            throw new InvalidDataException(
                "Unsupported settings version. The file has been left untouched."
            );
        }

        if (!Enum.IsDefined(Theme))
        {
            throw new InvalidDataException("Invalid appearance setting.");
        }

        // JSON can contain null properties and list entries despite the non-nullable annotations.
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Osd is null || Devices is null || Hotkeys is null || !Enum.IsDefined(DefaultDirection))
        {
            throw new InvalidDataException("Invalid settings structure.");
        }
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

        if (
            !double.IsFinite(Osd.Left)
            || !double.IsFinite(Osd.Top)
            || !double.IsFinite(Osd.Opacity)
            || Osd.Opacity < 0
            || Osd.Opacity > 1
            || Osd.Timeout < 100
            || Osd.Timeout > 100000
        )
        {
            throw new InvalidDataException("Invalid OSD settings.");
        }

        if (string.IsNullOrWhiteSpace(Osd.Skin) || Path.GetFileName(Osd.Skin) != Osd.Skin)
        {
            throw new InvalidDataException("Invalid skin name.");
        }

        if (
            FlyoutLeft is double x && !double.IsFinite(x)
            || FlyoutTop is double y && !double.IsFinite(y)
        )
        {
            throw new InvalidDataException("Invalid window position.");
        }

        if (
            Devices.Any(d =>
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                d is null
                || string.IsNullOrWhiteSpace(d.Id)
                || d.Hue is < 0 or > 360
                || d.Saturation is < -100 or > 100
                || d.Brightness is < -100 or > 100
            )
        )
        {
            throw new InvalidDataException("Invalid device preferences.");
        }

        if (
            Hotkeys.Any(h =>
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                h is null
                || !Enum.IsDefined(h.Function)
                || h.VirtualKey is < 1 or > 254
                || h.Modifiers > 15
            )
        )
        {
            throw new InvalidDataException("Invalid hotkey.");
        }

        if (Devices.GroupBy(d => d.Id).Any(g => g.Count() > 1))
        {
            throw new InvalidDataException("Duplicate device preferences.");
        }

        if (Hotkeys.GroupBy(h => (h.Modifiers, h.VirtualKey)).Any(g => g.Count() > 1))
        {
            throw new InvalidDataException("Duplicate hotkey assignments.");
        }
    }
}
