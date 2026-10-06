using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AudioSwitch.Core.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch.Input;

internal sealed class Shortcuts : IDisposable
{
    private static readonly (int VirtualKey, ScrollKeys Modifier)[] ScrollModifiers =
    [
        (1, ScrollKeys.LeftMouseButton),
        (2, ScrollKeys.RightMouseButton),
        (0x11, ScrollKeys.Control),
        (0x12, ScrollKeys.Alt),
        (0x10, ScrollKeys.Shift),
        (0x5b, ScrollKeys.LWin),
        (0x5c, ScrollKeys.RWin),
    ];

    private readonly TrayIcon tray;
    private readonly Dispatcher dispatcher;
    private Dictionary<int, HotkeySettings> active = [];
    private int nextId = 100;
    private nint mouseHook;
    private readonly Native.HookProc callback;
    private AppSettings settings = new();
    internal event Action<HotkeySettings>? Pressed;
    internal event Action<int>? Scrolled;

    internal Shortcuts(TrayIcon tray, Dispatcher dispatcher)
    {
        this.tray = tray;
        this.dispatcher = dispatcher;
        callback = Mouse;
        tray.Hotkey += OnHotkey;
    }

    private void OnHotkey(int id)
    {
        if (active.TryGetValue(id, out var hotkey))
        {
            Pressed?.Invoke(hotkey);
        }
    }

    internal void Apply(AppSettings value)
    {
        var duplicate = value
            .Hotkeys.GroupBy(h => (h.Modifiers, h.VirtualKey))
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException("Two actions use the same hotkey.");
        }

        // Release the old registrations first so unchanged shortcuts can be re-registered.
        // If any replacement fails, restore the entire previous set below.
        var old = active;
        foreach (var id in old.Keys)
        {
            Native.UnregisterHotKey(tray.Handle, id);
        }

        var replacement = new Dictionary<int, HotkeySettings>();
        try
        {
            foreach (var hotkey in value.Hotkeys)
            {
                var id = nextId++;
                if (
                    !Native.RegisterHotKey(
                        tray.Handle,
                        id,
                        hotkey.Modifiers | 0x4000,
                        (uint)hotkey.VirtualKey
                    )
                )
                {
                    throw new Win32Exception(
                        Marshal.GetLastWin32Error(),
                        $"Cannot register {hotkey.Function}. The shortcut may already be in use."
                    );
                }

                replacement.Add(id, hotkey);
            }

            if (value.VolumeScroll && mouseHook == 0)
            {
                mouseHook = Native.SetWindowsHookEx(14, callback, Native.GetModuleHandle(null), 0);
                if (mouseHook == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            if (!value.VolumeScroll && mouseHook != 0)
            {
                Native.UnhookWindowsHookEx(mouseHook);
                mouseHook = 0;
            }

            active = replacement;
            settings = value;
        }
        catch
        {
            foreach (var id in replacement.Keys)
            {
                Native.UnregisterHotKey(tray.Handle, id);
            }

            foreach (var (id, hotkey) in old)
            {
                Native.RegisterHotKey(
                    tray.Handle,
                    id,
                    hotkey.Modifiers | 0x4000,
                    (uint)hotkey.VirtualKey
                );
            }

            throw;
        }
    }

    private nint Mouse(int code, nint message, nint data)
    {
        if (code >= 0 && message == 0x20a && settings.VolumeScroll)
        {
            ScrollKeys pressed = 0;
            foreach (var (virtualKey, modifier) in ScrollModifiers)
            {
                if (Native.GetAsyncKeyState(virtualKey) < 0)
                {
                    pressed |= modifier;
                }
            }

            if (pressed == settings.ScrollKeys)
            {
                // MSLLHOOKSTRUCT.mouseData follows the 8-byte POINT; its high word is signed.
                var delta = (short)(Marshal.ReadInt32(data, 8) >> 16);
                // Return from the global hook promptly; COM and UI work run on the dispatcher.
                dispatcher.BeginInvoke(() => Scrolled?.Invoke(delta));
                return 1;
            }
        }

        return Native.CallNextHookEx(mouseHook, code, message, data);
    }

    public void Dispose()
    {
        tray.Hotkey -= OnHotkey;
        foreach (var id in active.Keys)
        {
            Native.UnregisterHotKey(tray.Handle, id);
        }

        if (mouseHook != 0)
        {
            Native.UnhookWindowsHookEx(mouseHook);
        }
    }
}
