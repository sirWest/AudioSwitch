using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Cli;

internal static partial class Program
{
    private static int ExecuteLegacy(
        List<string> args,
        AudioService audio,
        SettingsStore store,
        AppSettings settings
    )
    {
        var direction = settings.DefaultDirection;
        uint? modifiers = null;
        int? key = null;
        HotkeyAction? function = null;
        for (var i = 0; i < args.Count; i++)
        {
            string Value() =>
                i + 1 < args.Count
                    ? args[++i]
                    : throw new ArgumentException($"Missing value for {args[i]}.");
            switch (args[i].ToLowerInvariant())
            {
                case "/i":
                    direction = Direction.Recording;
                    break;
                case "/o":
                    direction = Direction.Playback;
                    break;
                case "/l":
                    ListDevices(audio, direction, settings, false);
                    break;
                case "/s":
                    var selector = Value();
                    AudioDevice device;
                    if (int.TryParse(selector, out var index))
                    {
                        var devices = audio
                            .List(direction, settings)
                            .Where(d => settings.ForDevice(d.Id)?.Hidden != true)
                            .ToArray();
                        if (index < 0 || index >= devices.Length)
                        {
                            throw new ArgumentException("Invalid device index.");
                        }

                        device = devices[index];
                    }
                    else
                    {
                        device = audio.Resolve(direction, selector, settings);
                    }

                    audio.Select(device, settings);
                    Console.WriteLine(device.DisplayName(settings));
                    break;
                case "/m":
                    if (i == args.Count - 1)
                    {
                        Console.WriteLine("None, Alt, Control, Shift, LWin, RWin");
                        return 0;
                    }

                    modifiers = ParseModifiers(Value());
                    break;
                case "/k":
                    if (i == args.Count - 1)
                    {
                        Console.WriteLine(
                            "A-Z, D0-D9, F1-F24, NumPad0-NumPad9, VolumeUp, VolumeDown, VolumeMute, or virtual-key number"
                        );
                        return 0;
                    }

                    key = ParseKey(Value());
                    break;
                case "/f":
                    if (i == args.Count - 1)
                    {
                        Console.WriteLine(string.Join(", ", Enum.GetNames<HotkeyAction>()));
                        return 0;
                    }

                    function = Enum.Parse<HotkeyAction>(Value(), true);
                    break;
                case "/startup":
                    audio.ApplyStartup(settings);
                    break;
                default:
                    throw new ArgumentException("Unknown legacy option: " + args[i]);
            }
        }

        if (modifiers is not null || key is not null || function is not null)
        {
            if (modifiers is null || key is null || function is null)
            {
                throw new ArgumentException("Specify /m, /k, and /f together.");
            }

            AddHotkey(settings, function.Value, key.Value, modifiers.Value, true);
            store.Save(settings);
        }

        return 0;
    }
}
