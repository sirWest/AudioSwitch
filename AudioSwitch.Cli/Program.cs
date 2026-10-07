using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Cli;

internal static partial class Program
{
    private const string Help = """
        AudioSwitch
        audioswitch-cli list [--input|--output] [--json] [--all]
        audioswitch-cli set <device-id|exact-name> [--input|--output] [--role multimedia|communications|console|all]
        audioswitch-cli next|previous [--input|--output]
        audioswitch-cli volume [0..100|+N|-N] [--input|--output]
        audioswitch-cli mute [on|off|toggle] [--input|--output]
        audioswitch-cli startup
        audioswitch-cli hotkeys [--json]
        audioswitch-cli hotkey-add <action> <key-name|virtual-key-number> <Alt,Control,Shift,Win|None> [--no-osd]
        audioswitch-cli hotkey-remove <zero-based-index>

        Legacy aliases: /i /o /l /s <index|name> /m <modifiers> /k <key> /f <action> /startup /help
        --settings <path> selects a separate settings file.
        Exit codes: 0 success, 2 invalid arguments, 3 operation/settings failure.
        Device IDs are preferred for scripts. Duplicate names are rejected.
        """;

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args.Any(a => a is "--help" or "-h" or "/help" or "/h" or "/?"))
            {
                Console.WriteLine(Help);
                return 0;
            }

            var tokens = args.ToList();
            var settingsPath = TakeOption(tokens, "--settings");
            var store = new SettingsStore(settingsPath);
            var settings = store.Load();
            if (store.RecoveryNotice is { } notice)
            {
                Console.Error.WriteLine(notice);
            }

            using var audio = new AudioService();
            if (tokens.Count == 0)
            {
                throw new ArgumentException("Specify a command. Use --help.");
            }

            if (tokens[0].StartsWith('/'))
            {
                return ExecuteLegacy(tokens, audio, store, settings);
            }

            return ExecuteCommand(tokens, settings, audio, store);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
        catch (Exception ex)
        {
            AudioDiagnostics.Log.Failure("CLI command", ex);
            Console.Error.WriteLine(ex.Message);
            return 3;
        }
    }
}
