namespace AudioSwitch.Core.Settings;

public sealed class OsdSettings
{
    public string Skin { get; set; } = "Default";
    public double Left { get; set; } = 61;
    public double Top { get; set; } = 26;
    public int Timeout { get; set; } = 1300;
    public double Opacity { get; set; } = 1;
}
