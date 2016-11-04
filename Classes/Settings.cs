using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using AudioSwitch.CoreAudioApi;

namespace AudioSwitch.Classes
{
    [XmlRoot, Serializable]
    public class Settings
    {
        public static Settings newSettings()
        {
            return new Settings
            {
                DefaultDataFlow = EDataFlow.eRender,
                //DefaultMultimediaAndComm = false,
                //ColorVU = false,
                ShowHardwareName = true,
                //QuickSwitchEnabled = false,
                QuickSwitchShowOSD = true,

                OSD = new COSD
                {
                    Skin = "Default",
                    Left = 61,
                    Top = 26,
                    ClosingTimeout = 1300,
                    Transparency = 255
                },

                ShowBothDataFlow = false,

                VolumeScroll = new CVolScroll
                {
                    Key = VolumeScrollKey.LWin,
                    ShowOSD = true
                    //, Enabled = false
                },
                Device = new List<CDevice>(),
                Hotkey = new BindingList<Hotkey>()
            };
        }

        [XmlIgnore]
        private static readonly string settingsxml = Program.AppDataRoot + "Settings.xml";

        [XmlElement]
        public EDataFlow DefaultDataFlow;

        [XmlElement]
        public bool DefaultMultimediaAndComm;

        [XmlElement]
        public bool ColorVU;

        [XmlElement]
        public bool ShowBothDataFlow;

        [XmlElement]
        public bool ShowHardwareName;

        [XmlElement]
        public bool QuickSwitchEnabled;

        [XmlElement]
        public bool QuickSwitchShowOSD;

        [XmlElement]
        public bool UseCustomOSD;

        [XmlElement]
        public COSD OSD;

        public class COSD
        {
            [XmlAttribute]
            public string Skin;

            [XmlAttribute]
            public int Left;

            [XmlAttribute]
            public int Top;

            [XmlAttribute]
            public int ClosingTimeout;

            [XmlAttribute]
            public byte Transparency;
        }

        [XmlElement]
        public CVolScroll VolumeScroll;

        public class CVolScroll
        {
            [XmlAttribute]
            public bool Enabled
            {
                get { return ScrollVolume.IsEnabled; }
                set { ScrollVolume.RegisterVolScroll(value); }
            }

            [XmlAttribute]
            public VolumeScrollKey Key;

            [XmlAttribute]
            public bool ShowOSD;
        }

        [XmlElement]
        public BindingList<Hotkey> Hotkey;

        [XmlElement]
        public List<CDevice> Device;
 
        public class CDevice
        {
            [XmlAttribute]
            public string DeviceID;

            [XmlAttribute]
            public bool HideFromList;

            [XmlAttribute]
            public int Hue;

            [XmlAttribute]
            public int Saturation;

            [XmlAttribute]
            public int Brightness;

            [XmlAttribute]
            public bool UseCustomName;

            [XmlAttribute]
            public string CustomName;
        }

        internal void Save()
        {
            var xs = new XmlSerializer(typeof(Settings));
            using (var tw = new StreamWriter(settingsxml))
                xs.Serialize(tw, this);
        }

        internal static Settings Load()
        {
            try
            {
                var xs = new XmlSerializer(typeof(Settings));
                using (var fileStream = new StreamReader(settingsxml))
                    return (Settings)xs.Deserialize(fileStream);
            }
            catch
            {
                var newsettings = newSettings();
                newsettings.Save();
                return newsettings;
            }
        }
    }
}
