using System.IO;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class DeviceIconStoreTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        "AudioSwitch.Tests",
        Guid.NewGuid().ToString("N")
    );
    private string Profile => Path.Combine(directory, "settings.json");

    [Fact]
    public void DirectoryIsLazyAndIdenticalImportsAreSharedByBothDirections()
    {
        var icons = new DeviceIconStore(Profile);
        var store = new SettingsStore(Profile);
        var settings = store.Load();
        icons.Save(store, settings, new Dictionary<string, byte[]>());
        Assert.False(Directory.Exists(icons.DirectoryPath));
        byte[] bytes = [1, 2, 3];
        var name = DeviceIconStore.Name(bytes);
        foreach (var direction in new[] { Direction.Playback, Direction.Recording })
            settings.Devices.Add(
                new()
                {
                    Id = direction.ToString(),
                    Direction = direction,
                    CustomIcon = name,
                    UseDeviceIconInTray = true,
                }
            );
        icons.Save(store, settings, new Dictionary<string, byte[]> { [name] = bytes });
        Assert.Single(Directory.GetFiles(icons.DirectoryPath));
        Assert.Equal(bytes, File.ReadAllBytes(icons.Resolve(name)));
        Assert.All(
            new SettingsStore(Profile).Load().Devices,
            d =>
            {
                Assert.Equal(name, d.CustomIcon);
                Assert.True(d.UseDeviceIconInTray);
            }
        );
    }

    [Fact]
    public void CleanupPreservesBackupAndOtherProfilesIncludingDisconnectedDevices()
    {
        var icons = new DeviceIconStore(Profile);
        var store = new SettingsStore(Profile);
        var settings = store.Load();
        byte[] bytes = [1, 2, 3];
        var name = DeviceIconStore.Name(bytes);
        settings.Devices.Add(new() { Id = "disconnected", CustomIcon = name });
        icons.Save(store, settings, new Dictionary<string, byte[]> { [name] = bytes });
        var otherPath = Path.Combine(directory, "another-profile.config");
        var other = new SettingsStore(otherPath);
        other.Load();
        other.Save(settings);
        settings.Devices.Clear();
        icons.Save(store, settings, new Dictionary<string, byte[]>());
        Assert.True(File.Exists(icons.Resolve(name)));
        icons.Save(store, settings, new Dictionary<string, byte[]>());
        Assert.True(File.Exists(icons.Resolve(name)));
        File.Delete(otherPath);
        File.WriteAllBytes(Path.Combine(directory, "AudioSwitch.exe"), [77, 90, 0, 0]);
        icons.Save(store, settings, new Dictionary<string, byte[]>());
        Assert.False(File.Exists(icons.Resolve(name)));
    }

    [Fact]
    public void FailedSaveDoesNotPruneAndUnreadableBackupDefersCleanup()
    {
        var icons = new DeviceIconStore(Profile);
        var stale = new SettingsStore(Profile);
        stale.Load();
        var store = new SettingsStore(Profile);
        var settings = store.Load();
        byte[] bytes = [4, 5, 6];
        var name = DeviceIconStore.Name(bytes);
        settings.Devices.Add(new() { Id = "device", CustomIcon = name });
        icons.Save(store, settings, new Dictionary<string, byte[]> { [name] = bytes });
        Assert.Throws<IOException>(() =>
            icons.Save(stale, new(), new Dictionary<string, byte[]>())
        );
        Assert.True(File.Exists(icons.Resolve(name)));
        settings.Devices.Clear();
        File.WriteAllText(Path.Combine(directory, "other.json.bak"), "broken");
        icons.Save(store, settings, new Dictionary<string, byte[]>());
        icons.Save(store, settings, new Dictionary<string, byte[]>());
        Assert.True(File.Exists(icons.Resolve(name)));
    }

    [Theory]
    [InlineData("../escape.png")]
    [InlineData("C:\\escape.png")]
    [InlineData("icon.png")]
    [InlineData(null)]
    public void InvalidReferencesAreRejected(string? name)
    {
        var settings = new AppSettings { Devices = [new() { Id = "device", CustomIcon = name! }] };
        Assert.Throws<InvalidDataException>(settings.Validate);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, true);
    }
}
