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
        [XmlIgnore] 
        internal static string settingsxml = "Settings.xml";

        [XmlElement]
        public bool IgnoreErrors;

        [XmlElement]
        public EDataFlow DefaultDataFlow;

        [XmlElement]
        public bool DefaultMultimediaAndComm;

        [XmlElement]
        public bool ColorVU;

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
            public int ClosingTimeout = 2000;

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
        }

        internal void Save()
        {
            var xs = new XmlSerializer(typeof(Settings));
            using (var tw = new StreamWriter(settingsxml))
                xs.Serialize(tw, this);
        }

        internal static Settings Load()
        {
            var xs = new XmlSerializer(typeof(Settings));
            using (var fileStream = new StreamReader(settingsxml))
                return (Settings)xs.Deserialize(fileStream);
        }
    }
}
