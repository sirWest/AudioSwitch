using System.Xml.Linq;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Core.Settings;

public static class LegacySettings
{
    public static string DefaultPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AudioSwitch",
            "Settings.xml"
        );

    public static AppSettings Import(string path, Func<string, int> keyParser)
    {
        var root =
            XDocument.Load(path).Root ?? throw new InvalidDataException("Empty legacy settings.");
        bool Flag(string name) => (bool?)root.Element(name) ?? false;
        var settings = new AppSettings
        {
            DefaultDirection =
                (string?)root.Element("DefaultDataFlow") == "eCapture"
                    ? Direction.Recording
                    : Direction.Playback,
            AlsoCommunications = Flag("DefaultMultimediaAndComm"),
            ColorVu = Flag("ColorVU"),
            ShowHardwareName = (bool?)root.Element("ShowHardwareName") ?? true,
            QuickSwitch = Flag("QuickSwitchEnabled"),
            QuickSwitchOsd = Flag("QuickSwitchShowOSD"),
            CloseAfterSelecting = Flag("CloseAfterSelecting"),
            AlwaysVisible = Flag("AlwaysVisible"),
            CustomOsd = (bool?)root.Element("UseCustomOSD") ?? true,
            FlyoutLeft = (double?)root.Element("FreePosLeft"),
            FlyoutTop = (double?)root.Element("FreePosTop"),
        };
        if (root.Element("OSD") is { } osd)
        {
            settings.Osd = new()
            {
                Skin = (string?)osd.Attribute("Skin") ?? "Default",
                Left = (double?)osd.Attribute("Left") ?? 61,
                Top = (double?)osd.Attribute("Top") ?? 26,
                Timeout = Math.Clamp((int?)osd.Attribute("ClosingTimeout") ?? 1300, 100, 100000),
                Opacity = Math.Clamp(((int?)osd.Attribute("Transparency") ?? 255) / 255d, 0, 1),
            };
        }

        if (root.Element("VolumeScroll") is { } scroll)
        {
            settings.VolumeScroll = (bool?)scroll.Attribute("Enabled") ?? false;
            settings.ScrollOsd = (bool?)scroll.Attribute("ShowOSD") ?? true;
            settings.ScrollKeys = Enum.Parse<ScrollKeys>(
                (string?)scroll.Attribute("Key") ?? "LWin"
            );
        }

        foreach (var device in root.Elements("Device"))
        {
            var id = (string?)device.Attribute("DeviceID");
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            settings.Devices.Add(
                new()
                {
                    Id = id,
                    Direction = id.StartsWith("{0.0.1.", StringComparison.Ordinal)
                        ? Direction.Recording
                        : Direction.Playback,
                    Hidden = (bool?)device.Attribute("HideFromList") ?? false,
                    UseCustomName = (bool?)device.Attribute("UseCustomName") ?? false,
                    CustomName = (string?)device.Attribute("CustomName") ?? "",
                    Hue = (int?)device.Attribute("Hue") ?? 0,
                    Saturation = (int?)device.Attribute("Saturation") ?? 0,
                    Brightness = (int?)device.Attribute("Brightness") ?? 0,
                    StartupMultimedia = (bool?)device.Attribute("DefaultMultimediaDevice") ?? false,
                    StartupCommunications =
                        (bool?)device.Attribute("DefaultCommunicationsDevice") ?? false,
                }
            );
        }

        foreach (var hotkey in root.Elements("Hotkey"))
        {
            uint modifiers = 0;
            foreach (
                var modifier in ((string?)hotkey.Attribute("ModifierKeys") ?? "").Split(
                    [' ', ','],
                    StringSplitOptions.RemoveEmptyEntries
                )
            )
            {
                modifiers |= modifier switch
                {
                    "Alt" => 1u,
                    "Control" => 2u,
                    "Shift" => 4u,
                    "LWin" or "RWin" => 8u,
                    _ => 0u,
                };
            }

            var key = keyParser((string?)hotkey.Attribute("HotKey") ?? "None");
            if (key == 0)
            {
                throw new InvalidDataException(
                    "A legacy hotkey could not be imported. Original settings are untouched."
                );
            }

            settings.Hotkeys.Add(
                new()
                {
                    Function = Enum.Parse<HotkeyAction>((string)hotkey.Attribute("Function")!),
                    VirtualKey = key,
                    Modifiers = modifiers,
                    ShowOsd = (bool?)hotkey.Attribute("ShowOSD") ?? false,
                }
            );
        }

        settings.Validate();
        return settings;
    }
}
