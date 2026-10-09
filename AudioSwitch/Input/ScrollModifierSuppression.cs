using AudioSwitch.Core.Settings;

namespace AudioSwitch.Input;

internal sealed class ScrollModifierSuppression
{
    private static readonly (int VirtualKey, ScrollKeys Modifier)[] KeyboardModifiers =
    [
        (0xa0, ScrollKeys.Shift),
        (0xa1, ScrollKeys.Shift),
        (0xa2, ScrollKeys.Control),
        (0xa3, ScrollKeys.Control),
        (0xa4, ScrollKeys.Alt),
        (0xa5, ScrollKeys.Alt),
        (0x5b, ScrollKeys.LWin),
        (0x5c, ScrollKeys.RWin),
    ];

    private readonly HashSet<int> usedKeys = [];

    internal bool MarkUsed(ScrollKeys modifiers, Func<int, bool> isDown)
    {
        var used = false;
        foreach (var (virtualKey, modifier) in KeyboardModifiers)
        {
            if ((modifiers & modifier) != 0 && isDown(virtualKey))
            {
                usedKeys.Add(virtualKey);
                used = true;
            }
        }
        return used;
    }

    internal bool ShouldSuppress(int virtualKey, bool released)
    {
        if (released)
        {
            usedKeys.Remove(virtualKey);
            return false;
        }

        // Auto-repeat after the mask key could make Alt/Win look like a new tap.
        return usedKeys.Contains(virtualKey);
    }

    internal void Clear() => usedKeys.Clear();
}
