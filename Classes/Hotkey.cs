using System;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace AudioSwitch.Classes
{
    public sealed class Hotkey : IDisposable
    {
        [XmlAttribute]
        public HotkeyFunction Function;

        [XmlAttribute]
        public HotModifierKeys ModifierKeys;

        [XmlAttribute]
        public Keys HotKey;

        [XmlAttribute]
        public bool ShowOSD { get; set; }
        
        [XmlIgnore]
        internal short ID;

        [XmlIgnore]
        public string Key
        {
            get { return HotKey.ToString(); }
            set { HotKey = (Keys) Enum.Parse(typeof (Keys), value); }
        }

        [XmlIgnore]
        public string HKFunction
        {
            get { return Function.ToString(); }
            set { Function = (HotkeyFunction)Enum.Parse(typeof(HotkeyFunction), value); }
        }

        [XmlIgnore]
        public bool Control
        {
            get { return ModifierKeys.HasFlag(HotModifierKeys.Control); }
            set { ModHotFlag(value, HotModifierKeys.Control); }
        }

        [XmlIgnore]
        public bool Alt
        {
            get { return ModifierKeys.HasFlag(HotModifierKeys.Alt); }
            set { ModHotFlag(value, HotModifierKeys.Alt); }
        }

        [XmlIgnore]
        public bool Shift
        {
            get { return ModifierKeys.HasFlag(HotModifierKeys.Shift); }
            set { ModHotFlag(value, HotModifierKeys.Shift); }
        }

        [XmlIgnore]
        public bool Win
        {
            get { return ModifierKeys.HasFlag(HotModifierKeys.Win); }
            set { ModHotFlag(value, HotModifierKeys.Win); }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unregister();
            }
        }

        private void ModHotFlag(bool enabled, HotModifierKeys toggleEnum)
        {
            if (enabled)
                ModifierKeys |= toggleEnum;
            else
                ModifierKeys &= ~toggleEnum;
        }

        internal bool Register()
        {
            if (HotKey == Keys.None)
                return false;

            if (ID != 0)
                Unregister();

            ID = GlobalHotkeys.RegisterGlobalHotKey(ID, ModifierKeys, HotKey);
            return ID != 0;
        }

        internal void Unregister()
        {
            GlobalHotkeys.UnregisterGlobalHotKey(ID);
        }
    }

    public enum HotkeyFunction
    {
        SwitchPlaybackDevice,
        SwitchRecordingDevice,
        TogglePlaybackMute,
        ToggleRecordingMute,
        PlaybackVolumeUp,
        PlaybackVolumeDown,
        RecordingVolumeUp,
        RecordingVolumeDown
    }

    [Flags]
    public enum VolumeScrollKey
    {
        LeftMouseButton = 1,
        RightMouseButton = 2,
        Control = 4,
        Alt = 8,
        LWin = 16,
        RWin = 32,
        Shift = 64
    }
}
