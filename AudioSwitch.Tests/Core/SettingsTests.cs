using System.IO;
using System.Text.Json;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Placement;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class SettingsTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(),
        "AudioSwitch.Tests",
        Guid.NewGuid().ToString("N")
    );
    private string FilePath => Path.Combine(directory, "settings.json");

    [Theory]
    [InlineData(Direction.Playback)]
    [InlineData(Direction.Recording)]
    public void DefaultDevicePreferencesAreOmittedWithoutChangingTheEditor(Direction direction)
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        var device = new DeviceSettings { Id = "device", Direction = direction };
        settings.Devices.Add(device);

        store.Save(settings);

        Assert.Empty(new SettingsStore(FilePath).Load().Devices);
        Assert.Same(device, Assert.Single(settings.Devices));
    }

    [Fact]
    public void ExistingDefaultDevicePreferencesAreRemovedOnNextSave()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(
            FilePath,
            """
            {"Devices":[
              {"Id":"speakers","Direction":"Playback"},
              {"Id":"microphone","Direction":"Recording"},
              {"Id":"custom","Hidden":true}
            ]}
            """
        );
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        Assert.Equal(3, settings.Devices.Count);

        store.Save(settings);

        var device = Assert.Single(new SettingsStore(FilePath).Load().Devices);
        Assert.Equal("custom", device.Id);
        Assert.True(device.Hidden);
    }

    [Theory]
    [InlineData("\"Hidden\":true")]
    [InlineData("\"ExcludeFromHotkeyMute\":true")]
    [InlineData("\"UseCustomName\":true")]
    [InlineData("\"CustomName\":\"Remembered name\"")]
    [InlineData("\"Hue\":1")]
    [InlineData("\"Saturation\":-1")]
    [InlineData("\"Brightness\":-1")]
    [InlineData("\"StartupMultimedia\":true")]
    [InlineData("\"StartupCommunications\":true")]
    public void EachCustomDevicePreferenceIsPreservedUntilReverted(string preference)
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        var device = JsonSerializer.Deserialize<DeviceSettings>(
            "{\"Id\":\"device\",\"Direction\":\"Recording\"," + preference + "}",
            SettingsStore.Json
        )!;
        settings.Devices.Add(device);

        store.Save(settings);

        var restored = Assert.Single(new SettingsStore(FilePath).Load().Devices);
        Assert.Equal(
            JsonSerializer.Serialize(device, SettingsStore.Json),
            JsonSerializer.Serialize(restored, SettingsStore.Json)
        );

        settings.Devices[0] = new() { Id = device.Id, Direction = device.Direction };
        store.Save(settings);

        Assert.Empty(new SettingsStore(FilePath).Load().Devices);
    }

    [Fact]
    public void DeviceGroupDefaultsToPlaybackAndBothSurvivesSaveAndClone()
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        Assert.Equal(DeviceGroup.Playback, settings.DefaultDeviceGroup);
        settings.DefaultDeviceGroup = DeviceGroup.Recording;
        store.Save(settings);
        Assert.Equal(DeviceGroup.Recording, new SettingsStore(FilePath).Load().DefaultDeviceGroup);
        settings.DefaultDeviceGroup = DeviceGroup.Both;
        store.Save(settings);
        var restored = new SettingsStore(FilePath).Load().Clone();
        Assert.Equal(DeviceGroup.Both, restored.DefaultDeviceGroup);
        Assert.Equal(Direction.Playback, restored.DefaultDirection);
        restored.DefaultDeviceGroup = DeviceGroup.Playback;
        Assert.False(restored.ShowBothDeviceGroups);
    }

    [Fact]
    public void AppearanceDefaultsToSystemAndPersistsAnOverride()
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        Assert.Equal(AppTheme.System, settings.Theme);
        settings.Theme = AppTheme.Dark;
        store.Save(settings);
        Assert.Equal(AppTheme.Dark, new SettingsStore(FilePath).Load().Theme);
    }

    [Theory]
    [InlineData(900, 1040, 40, 40, 0, 0, 1920, 1040, 780, 832)]
    [InlineData(900, 0, 40, 40, 0, 40, 1920, 1040, 780, 48)]
    [InlineData(0, 500, 40, 40, 40, 0, 1880, 1080, 48, 420)]
    [InlineData(1880, 500, 40, 40, 0, 0, 1880, 1080, 1592, 420)]
    public void FlyoutIsCenteredOnIconWithGapAtEveryTaskbarEdge(
        double x,
        double y,
        double w,
        double h,
        double wx,
        double wy,
        double ww,
        double wh,
        double expectedX,
        double expectedY
    )
    {
        var position = WindowPlacement.NearAnchor(new(x, y, w, h), new(wx, wy, ww, wh), 280, 200);
        Assert.Equal(expectedX, position.X);
        Assert.Equal(expectedY, position.Y);
    }

    [Fact]
    public void RoundTripAndPreviousGoodBackup()
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        settings.Osd.Skin = "Ice";
        store.Save(settings);
        settings.Osd.Skin = "PiX";
        store.Save(settings);
        Assert.Equal("PiX", new SettingsStore(FilePath).Load().Osd.Skin);
        Assert.Contains("Ice", File.ReadAllText(FilePath + ".bak"));
    }

    [Fact]
    public void CorruptionRecoversBackupAndPreservesDamagedFile()
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        settings.Osd.Skin = "Ice";
        store.Save(settings);
        store.Save(settings);
        File.WriteAllText(FilePath, "{ broken");
        var recovered = new SettingsStore(FilePath);
        var value = recovered.Load();
        Assert.Equal("Ice", value.Osd.Skin);
        Assert.NotNull(recovered.RecoveryNotice);
        recovered.Save(value);
        Assert.Single(Directory.GetFiles(directory, "*.damaged-*"));
        Assert.Contains("Ice", File.ReadAllText(FilePath + ".bak"));
    }

    [Fact]
    public void CorruptionWithoutBackupIsNeverReplaced()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(FilePath, "bad");
        Assert.Throws<InvalidDataException>(() => new SettingsStore(FilePath).Load());
        Assert.Equal("bad", File.ReadAllText(FilePath));
    }

    [Fact]
    public void ConcurrentEditorCannotOverwriteCliChange()
    {
        var first = new SettingsStore(FilePath);
        var value = first.Load();
        first.Save(value);
        var second = new SettingsStore(FilePath);
        var changed = second.Load();
        changed.ColorVu = true;
        second.Save(changed);
        Assert.Throws<IOException>(() => first.Save(value));
        Assert.True(new SettingsStore(FilePath).Load().ColorVu);
    }

    [Theory]
    [InlineData("{\"Osd\":null}")]
    [InlineData("{\"Devices\":null}")]
    [InlineData("{\"Hotkeys\":null}")]
    [InlineData("{\"Devices\":[null]}")]
    [InlineData("{\"Hotkeys\":[null]}")]
    public void NullSettingsValuesWithoutBackupAreRejectedAndPreserved(string json)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(FilePath, json);

        Assert.Throws<InvalidDataException>(() => new SettingsStore(FilePath).Load());
        Assert.Equal(json, File.ReadAllText(FilePath));
    }

    [Theory]
    [InlineData("{\"Osd\":null}")]
    [InlineData("{\"Devices\":null}")]
    [InlineData("{\"Hotkeys\":null}")]
    [InlineData("{\"Devices\":[null]}")]
    [InlineData("{\"Hotkeys\":[null]}")]
    public void NullSettingsValuesRecoverBackupAndPreserveDamagedFile(string json)
    {
        var store = new SettingsStore(FilePath);
        var settings = store.Load();
        settings.Osd.Skin = "Ice";
        store.Save(settings);
        store.Save(settings);
        File.WriteAllText(FilePath, json);

        var recovered = new SettingsStore(FilePath);
        var value = recovered.Load();
        Assert.Equal("Ice", value.Osd.Skin);
        Assert.NotNull(recovered.RecoveryNotice);
        Assert.Equal(json, File.ReadAllText(FilePath));

        recovered.Save(value);
        var damagedFile = Assert.Single(Directory.GetFiles(directory, "*.damaged-*"));
        Assert.Equal(json, File.ReadAllText(damagedFile));
        Assert.Equal("Ice", new SettingsStore(FilePath).Load().Osd.Skin);
    }

    [Fact]
    public void FutureSettingsAreNotDowngradedToBackup()
    {
        var store = new SettingsStore(FilePath);
        var value = store.Load();
        store.Save(value);
        store.Save(value);
        File.WriteAllText(FilePath, "{\"Version\":99}");
        Assert.Throws<InvalidDataException>(() => new SettingsStore(FilePath).Load());
        Assert.Contains("99", File.ReadAllText(FilePath));
    }

    [Fact]
    public void ImportsLegacyDevicesHotkeysAndPreviewPosition()
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "legacy.xml");
        File.WriteAllText(
            path,
            """
            <Settings><DefaultDataFlow>eCapture</DefaultDataFlow><OSD Skin="Ice" Left="-500" Top="50" ClosingTimeout="1400" Transparency="128" />
            <Device DeviceID="{0.0.1.abc}" HideFromList="true" UseCustomName="true" CustomName="Mic" DefaultCommunicationsDevice="true" />
            <Hotkey Function="NextPlaybackDevice" ModifierKeys="Control RWin" HotKey="F8" ShowOSD="true" />
            <VolumeScroll Enabled="true" Key="Control" ShowOSD="true" /></Settings>
            """
        );
        var settings = LegacySettings.Import(path, _ => 119);
        Assert.Equal(Direction.Recording, settings.DefaultDirection);
        Assert.Equal(-500, settings.Osd.Left);
        Assert.True(settings.Devices.Single().Hidden);
        Assert.Equal(10u, settings.Hotkeys.Single().Modifiers);
        Assert.True(settings.VolumeScroll);
    }

    [Theory]
    [InlineData(1900, 1040, 20, 40, 0, 0, 1920, 1040)]
    [InlineData(0, 0, 40, 20, 0, 40, 1920, 1040)]
    [InlineData(-1920, 500, 40, 20, -1880, 0, 1880, 1080)]
    [InlineData(1880, 500, 40, 20, 0, 0, 1880, 1080)]
    public void TrayPlacementFitsWorkArea(
        double x,
        double y,
        double w,
        double h,
        double wx,
        double wy,
        double ww,
        double wh
    )
    {
        var position = WindowPlacement.NearAnchor(new(x, y, w, h), new(wx, wy, ww, wh), 280, 400);
        Assert.InRange(position.X, wx, wx + ww - 280);
        Assert.InRange(position.Y, wy, wy + wh - 400);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }
}
