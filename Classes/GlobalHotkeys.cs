using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AudioSwitch.Classes
{
    [Flags]
    public enum HotModifierKeys : uint
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        LWin = 8,
        RWin = 16
    }

    public static class GlobalHotkeys
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern void UnregisterHotKey(IntPtr hwnd, int id);

        internal static IntPtr Handle;
        private static short HotKeyIDCounter = 12345;

        internal static short RegisterGlobalHotKey(short oldID, HotModifierKeys modifierKeys, Keys hotKey)
        {
            if (oldID != 0)
                UnregisterGlobalHotKey(oldID);

            try
            {
                var newID = HotKeyIDCounter++;
                if (!RegisterHotKey(Handle, newID, (uint)modifierKeys, (uint)hotKey))
                        return 0;
                return newID;
            }
            catch
            {
                return 0;
            }
        }

        internal static void UnregisterGlobalHotKey(short id)
        {
            UnregisterHotKey(Handle, id);
        }

        internal static Hotkey AddOrFind(HotkeyFunction hotkeyFunction)
        {
            var hkey = Program.settings.Hotkey.ToList().Find(x => x.Function == hotkeyFunction);
            if (hkey == null)
                Program.settings.Hotkey.Add(hkey = new Hotkey { Function = hotkeyFunction });
            return hkey;
        }
        
        internal static void RegisterAll()
        {
            foreach (var hotkey in Program.settings.Hotkey)
                hotkey.Register();
        }

        internal static void UnregisterAll()
        {
            foreach (var hotkey in Program.settings.Hotkey)
                hotkey.Unregister();
        }
    }
}