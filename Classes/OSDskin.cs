using System;
using System.IO;
using System.Xml.Serialization;

namespace AudioSwitch.Classes
{
    [XmlRoot, Serializable]
    public class OSDskin
    {
		[XmlIgnore]
		public static string SkinsRootDir
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Skins");
			}
		}

        [XmlAttribute] public string Version;
        [XmlElement] public string Author;
        [XmlElement] public string Website;
        [XmlElement] public CVolMeter VolBar;

        public class CVolMeter
        {
            [XmlAttribute] public string Type;
            [XmlAttribute] public int Steps;
            [XmlAttribute] public int X;
            [XmlAttribute] public int Y;
            [XmlAttribute] public bool Effect;
        }

        [XmlElement] public CDeviceText DeviceText;

        public class CDeviceText
        {
            [XmlAttribute] public int X;
            [XmlAttribute] public int Y;
            [XmlAttribute] public string Font;
            [XmlAttribute] public string ColorHex;
            [XmlAttribute] public int FontSize;
            [XmlAttribute] public int MaxWidth;
            [XmlAttribute] public int MaxHeight;
        }

        internal static OSDskin Load(string SkinName)
        {
            var xs = new XmlSerializer(typeof(OSDskin));
            using (var fileStream = new StreamReader(Path.Combine(SkinsRootDir, Program.settings.OSD.Skin, "skin.xml")))
                return (OSDskin)xs.Deserialize(fileStream);
        }
    }
}
