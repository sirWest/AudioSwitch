using System.Windows.Data;
using System.Windows.Input;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Input;

internal sealed class ShortcutConverter : IValueConverter
{
    internal static string Format(HotkeySettings hotkey)
    {
        var modifiers =
            hotkey.Modifiers == 0
                ? ""
                : ((ModifierKeys)hotkey.Modifiers).ToString().Replace(", ", " + ") + " + ";
        var key =
            hotkey.VirtualKey == 0
                ? ""
                : KeyInterop.KeyFromVirtualKey(hotkey.VirtualKey).ToString();
        return modifiers + key;
    }

    public object Convert(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture
    ) => value is HotkeySettings hotkey ? Format(hotkey) : "";

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture
    ) => throw new NotSupportedException();
}
