using System.IO;
using System.Xml.Linq;

namespace AudioSwitch.Osd;

internal static class OsdSkin
{
    internal static string ImagePath(
        string directory,
        XElement skin,
        bool dark,
        string name,
        string fallback
    )
    {
        var images = skin.Element("Images");
        var file =
            (string?)images?.Element(dark ? "Dark" : "Light")?.Attribute(name)
            ?? (string?)images?.Attribute(name)
            ?? fallback;
        var root = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(directory, file));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Skin images must be inside the skin directory.");
        }

        return path;
    }

    internal static string TextColor(XElement element, bool dark) =>
        (string?)element.Attribute(dark ? "ColorHexDark" : "ColorHexLight")
        ?? (string?)element.Attribute("ColorHex")
        ?? "#FFFFFF";
}
