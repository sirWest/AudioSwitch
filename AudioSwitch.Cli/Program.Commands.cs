using System.Globalization;
using System.Text.Json;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Cli;

internal static partial class Program
{
    private static int ExecuteCommand(
        List<string> tokens,
        AppSettings settings,
        AudioService audio,
        SettingsStore store
    )
    {
        var json = tokens.Remove("--json");
        var all = tokens.Remove("--all");
        var noOsd = tokens.Remove("--no-osd");
        var input = tokens.Remove("--input");
        var output = tokens.Remove("--output");
        if (input && output)
        {
            throw new ArgumentException("Choose either --input or --output.");
        }

        var direction =
            input ? Direction.Recording
            : output ? Direction.Playback
            : settings.DefaultDirection;
        var role = TakeOption(tokens, "--role");
        if (tokens.Count == 0)
        {
            throw new ArgumentException("Specify a command. Use --help.");
        }

        var command = tokens[0].ToLowerInvariant();
        tokens.RemoveAt(0);
        if (role is not null && command != "set")
        {
            throw new ArgumentException("--role applies only to set.");
        }

        if (all && command != "list")
        {
            throw new ArgumentException("--all applies only to list.");
        }

        if (noOsd && command != "hotkey-add")
        {
            throw new ArgumentException("--no-osd applies only to hotkey-add.");
        }

        switch (command)
        {
            case "list":
                RequireArgumentCount(tokens, 0);
                ListDevices(audio, direction, settings, json, all);
                break;
            case "set":
                RequireArgumentCount(tokens, 1);
                var device = audio.Resolve(direction, tokens[0], settings);
                if (role is null)
                {
                    audio.Select(device, settings);
                }
                else
                {
                    audio.SetDefault(
                        device.Id,
                        role.Equals("all", StringComparison.OrdinalIgnoreCase)
                            ? Enum.GetValues<AudioRole>()
                            : [ParseRole(role)]
                    );
                }

                WriteResult(
                    new { device.Id, Name = device.DisplayName(settings) },
                    json,
                    device.DisplayName(settings)
                );
                break;
            case "next":
            case "previous":
                RequireArgumentCount(tokens, 0);
                var next = audio.Cycle(direction, command == "previous", settings);
                WriteResult(next, json, next.DisplayName(settings));
                break;
            case "volume":
                Volume(tokens, audio, direction, json);
                break;
            case "mute":
                if (tokens.Count > 1)
                {
                    throw new ArgumentException("mute accepts at most one value.");
                }

                var muteId = RequireDefault(audio, direction);
                var muted =
                    tokens.Count == 0
                        ? audio.State(muteId)
                        : audio.SetMute(
                            muteId,
                            tokens[0].ToLowerInvariant() switch
                            {
                                "on" => true,
                                "off" => false,
                                "toggle" => null,
                                _ => throw new ArgumentException("Use on, off, or toggle."),
                            }
                        );
                WriteResult(muted, json, muted.Muted ? "on" : "off");
                break;
            case "startup":
                RequireArgumentCount(tokens, 0);
                audio.ApplyStartup(settings);
                break;
            case "hotkeys":
                RequireArgumentCount(tokens, 0);
                Console.WriteLine(JsonSerializer.Serialize(settings.Hotkeys, SettingsStore.Json));
                break;
            case "hotkey-add":
                RequireArgumentCount(tokens, 3);
                AddHotkey(
                    settings,
                    Enum.Parse<HotkeyAction>(tokens[0], true),
                    ParseKey(tokens[1]),
                    ParseModifiers(tokens[2]),
                    !noOsd
                );
                store.Save(settings);
                break;
            case "hotkey-remove":
                RequireArgumentCount(tokens, 1);
                if (
                    !int.TryParse(tokens[0], out var index)
                    || index < 0
                    || index >= settings.Hotkeys.Count
                )
                {
                    throw new ArgumentException("Invalid hotkey index.");
                }

                settings.Hotkeys.RemoveAt(index);
                store.Save(settings);
                break;
            default:
                throw new ArgumentException($"Unknown command: {command}");
        }

        return 0;
    }

    private static void Volume(
        List<string> tokens,
        AudioService audio,
        Direction direction,
        bool json
    )
    {
        if (tokens.Count > 1)
        {
            throw new ArgumentException("volume accepts at most one value.");
        }

        var id = RequireDefault(audio, direction);
        var state = audio.State(id);
        if (tokens.Count == 1)
        {
            if (
                !float.TryParse(
                    tokens[0],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var number
                ) || !float.IsFinite(number)
            )
            {
                throw new ArgumentException("Invalid volume.");
            }

            var relative = tokens[0].StartsWith('+') || tokens[0].StartsWith('-');
            if (!relative && number is < 0 or > 100)
            {
                throw new ArgumentException("Volume must be 0..100.");
            }

            state = audio.SetVolume(id, relative ? state.Volume + number / 100 : number / 100);
        }

        WriteResult(
            state,
            json,
            (state.Volume * 100).ToString("0.##", CultureInfo.InvariantCulture)
        );
    }

    private static string RequireDefault(AudioService audio, Direction direction) =>
        audio.DefaultId(direction)
        ?? throw new InvalidOperationException("No default audio device.");

    private static void WriteResult(object value, bool json, string text) =>
        Console.WriteLine(json ? JsonSerializer.Serialize(value, SettingsStore.Json) : text);

    private static void ListDevices(
        AudioService audio,
        Direction direction,
        AppSettings settings,
        bool json,
        bool all = false
    )
    {
        var devices = audio
            .List(direction, settings)
            .Where(d => all || settings.ForDevice(d.Id)?.Hidden != true)
            .ToArray();
        if (json)
        {
            Console.WriteLine(
                JsonSerializer.Serialize(
                    devices.Select(d => new
                    {
                        d.Id,
                        Name = d.DisplayName(settings),
                        SystemName = d.Name,
                        d.Direction,
                        d.Multimedia,
                        d.Communications,
                        d.Console,
                    }),
                    SettingsStore.Json
                )
            );
            return;
        }

        for (var i = 0; i < devices.Length; i++)
        {
            Console.WriteLine(
                $"{(devices[i].Multimedia ? '*' : ' ')} {i} {devices[i].DisplayName(settings)}\n    {devices[i].Id}"
            );
        }
    }

    private static void AddHotkey(
        AppSettings settings,
        HotkeyAction action,
        int key,
        uint modifiers,
        bool osd
    )
    {
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentException("Invalid action.");
        }

        if (
            settings.Hotkeys.Any(h =>
                h.Function != action && h.VirtualKey == key && h.Modifiers == modifiers
            )
        )
        {
            throw new ArgumentException("Shortcut already assigned.");
        }

        settings.Hotkeys.RemoveAll(h => h.Function == action);
        settings.Hotkeys.Add(
            new()
            {
                Function = action,
                VirtualKey = key,
                Modifiers = modifiers,
                ShowOsd = osd,
            }
        );
    }
}
