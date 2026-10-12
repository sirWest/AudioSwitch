using System.IO;
using AudioSwitch.Shell;
using Microsoft.Win32;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class TrayVisibilityTests
{
    [Fact]
    public void ResolvesEnvironmentVariablesAndKnownFolders()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AudioSwitch", "AudioSwitch.exe");
        Assert.Equal(expected, TrayVisibility.ResolvePath(@"%LOCALAPPDATA%\AudioSwitch\AudioSwitch.exe"));
        Assert.Equal(expected, TrayVisibility.ResolvePath(
            @"{F1B32785-6FBA-4FCF-9D55-7B8E7F157091}\AudioSwitch\AudioSwitch.exe"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AudioSwitch.exe")]
    [InlineData(@"{invalid}\AudioSwitch.exe")]
    [InlineData(@"{F1B32785-6FBA-4FCF-9D55-7B8E7F157091}AudioSwitch.exe")]
    public void InvalidPathsAreIgnored(string path) =>
        Assert.Null(TrayVisibility.ResolvePath(path));

    [Fact]
    public void PromotesOnlyTheMatchingInstallationAndCanBeRepeated()
    {
        var key = @"Software\AudioSwitch.Tests\" + Guid.NewGuid();
        using var user = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        try
        {
            using var root = user.CreateSubKey(key);
            const string installed = @"C:\Apps with spaces\AudioSwitch\AudioSwitch.exe";
            Assert.False(TrayVisibility.Promote(root, installed));
            using var other = root.CreateSubKey("portable");
            other.SetValue("ExecutablePath", @"C:\Portable\AudioSwitch.exe");
            other.SetValue("IsPromoted", 0);
            using var malformed = root.CreateSubKey("malformed");
            malformed.SetValue("ExecutablePath", new byte[] { 1, 2 });
            Assert.False(TrayVisibility.Promote(root, installed));
            using var matching = root.CreateSubKey("installed");
            matching.SetValue("ExecutablePath", installed.ToUpperInvariant());
            Assert.True(TrayVisibility.Promote(root, installed));
            Assert.Equal(1, matching.GetValue("IsPromoted"));
            Assert.Equal(RegistryValueKind.DWord, matching.GetValueKind("IsPromoted"));
            matching.SetValue("IsPromoted", 0);
            Assert.True(TrayVisibility.Promote(root, installed));
            Assert.True(TrayVisibility.Promote(root, installed));
            Assert.Equal(1, matching.GetValue("IsPromoted"));
            Assert.Equal(0, other.GetValue("IsPromoted"));
            Assert.Null(malformed.GetValue("IsPromoted"));
        }
        finally
        {
            user.DeleteSubKeyTree(key, throwOnMissingSubKey: false);
        }
    }
}
