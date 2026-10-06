using AudioSwitch.Core.Audio;

namespace AudioSwitch.Cli;

internal static partial class Program
{
    private static AudioRole ParseRole(string value) =>
        Enum.TryParse<AudioRole>(value, true, out var role) && Enum.IsDefined(role)
            ? role
            : throw new ArgumentException("Invalid role.");

    private static void RequireArgumentCount(List<string> tokens, int count)
    {
        if (tokens.Count != count)
        {
            throw new ArgumentException($"Expected {count} argument(s). Use --help.");
        }
    }

    private static string? TakeOption(List<string> tokens, string name)
    {
        var index = tokens.IndexOf(name);
        if (index < 0)
        {
            return null;
        }

        if (index == tokens.Count - 1)
        {
            throw new ArgumentException($"Missing value for {name}.");
        }

        var value = tokens[index + 1];
        tokens.RemoveRange(index, 2);
        return value;
    }

    private static uint ParseModifiers(string text)
    {
        uint value = 0;
        foreach (var token in text.Split([',', '+', ' '], StringSplitOptions.RemoveEmptyEntries))
        {
            value |= token.ToLowerInvariant() switch
            {
                "none" => 0u,
                "alt" => 1u,
                "control" or "ctrl" => 2u,
                "shift" => 4u,
                "win" or "lwin" or "rwin" or "windows" => 8u,
                _ => throw new ArgumentException("Invalid modifier."),
            };
        }

        return value;
    }

    private static int ParseKey(string value)
    {
        if (int.TryParse(value, out var number) && number is > 0 and < 255)
        {
            return number;
        }

        if (value.Length == 1 && char.IsAsciiLetter(value[0]))
        {
            return char.ToUpperInvariant(value[0]);
        }

        if (value.Length == 2 && value[0] == 'D' && char.IsAsciiDigit(value[1]))
        {
            return value[1];
        }

        if (value.StartsWith('F') && int.TryParse(value[1..], out var f) && f is >= 1 and <= 24)
        {
            return 0x70 + f - 1;
        }

        if (
            value.StartsWith("NumPad", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value[6..], out var n)
            && n is >= 0 and <= 9
        )
        {
            return 0x60 + n;
        }

        if (Enum.TryParse<ConsoleKey>(value, true, out var consoleKey))
        {
            return (int)consoleKey;
        }

        return value.ToLowerInvariant() switch
        {
            "volumeup" => 0xaf,
            "volumedown" => 0xae,
            "volumemute" => 0xad,
            "space" => 0x20,
            "return" => 0x0d,
            "lwin" => 0x5b,
            "rwin" => 0x5c,
            "oemtilde" => 0xc0,
            "oemcomma" => 0xbc,
            "oemperiod" => 0xbe,
            _ => throw new ArgumentException("Unknown key. Use its virtual-key number."),
        };
    }
}
