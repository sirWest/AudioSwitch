using System.Runtime.InteropServices;
using System.Text.Json;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class DeviceHotkeyTests
{
    [Fact]
    public void PairedShortcutUnmutesMicrophoneAfterPlaybackHasSwitched()
    {
        var rig = new Rig();
        rig.Run(Pair());
        Assert.Equal(
            new[]
            {
                "select:mic:False",
                "select:speakers:False",
                "mute:speakers:False",
                "mute:mic:False",
            },
            rig.Calls
        );
    }

    [Fact]
    public void SpeakersMuteAllMicrophonesBeforeSwitchingIncludingHiddenDevices()
    {
        var rig = new Rig();
        rig.Settings.Devices.Add(new() { Id = "mic", Hidden = true });
        rig.Settings.Devices.Add(new() { Id = "special", ExcludeFromHotkeyMute = true });
        rig.Devices.Add(Device("special", Direction.Recording));
        var hotkey = new HotkeySettings
        {
            Playback = new() { DeviceId = "speakers", MuteAction = HotkeyMuteAction.Unmute },
            Recording = new() { MuteOthers = true },
        };

        rig.Run(hotkey);

        Assert.Equal(
            new[] { "mute:mic:True", "select:speakers:False", "mute:speakers:False" },
            rig.Calls
        );
    }

    [Theory]
    [InlineData(HotkeyMuteAction.KeepCurrentState)]
    [InlineData(HotkeyMuteAction.Mute)]
    [InlineData(HotkeyMuteAction.Unmute)]
    public void RepeatedPressesApplyExplicitStateWithoutToggling(HotkeyMuteAction action)
    {
        var rig = new Rig();
        var hotkey = new HotkeySettings
        {
            Recording = new() { DeviceId = "mic", MuteAction = action },
        };
        rig.Run(hotkey);
        var first = rig.Calls.ToArray();
        rig.Calls.Clear();
        rig.Run(hotkey);
        Assert.Equal(first, rig.Calls);
        if (action == HotkeyMuteAction.KeepCurrentState)
        {
            Assert.Equal(new[] { "select:mic:False" }, first);
        }
        else
        {
            Assert.Contains($"mute:mic:{action == HotkeyMuteAction.Mute}", first);
            Assert.Equal(2, first.Length);
        }
    }

    [Theory]
    [InlineData(Direction.Playback)]
    [InlineData(Direction.Recording)]
    public void MissingHalfOfPairDoesNotBlockOtherHalfAndReconnectWorks(Direction missing)
    {
        var rig = new Rig();
        var removed = rig.Devices.Single(d => d.Direction == missing);
        rig.Devices.Remove(removed);
        var hotkey = Pair();
        rig.Run(hotkey);
        Assert.Contains(rig.Calls, call => call.StartsWith("select:"));
        Assert.DoesNotContain(rig.Calls, call => call.Contains(removed.Id));
        rig.Devices.Add(removed);
        rig.Calls.Clear();
        rig.Run(hotkey);
        Assert.Contains("select:mic:False", rig.Calls);
        Assert.Contains("select:speakers:False", rig.Calls);
    }

    [Fact]
    public void MissingTargetDoesNotMuteOtherDevicesInItsCategory()
    {
        var rig = new Rig();
        var completed = rig.Run(
            new()
            {
                Recording = new()
                {
                    DeviceId = "missing",
                    MuteOthers = true,
                    MuteAction = HotkeyMuteAction.Unmute,
                },
            }
        );
        Assert.Empty(rig.Calls);
        Assert.Empty(completed);
    }

    [Fact]
    public void EndpointDisappearingDuringBulkMuteDoesNotBlockRemainingDevices()
    {
        var rig = new Rig();
        rig.Devices.Add(Device("second", Direction.Recording));
        rig.Fail = call => call == "mute:mic:True";
        rig.Run(
            new()
            {
                Recording = new() { MuteOthers = true },
                Playback = new() { DeviceId = "speakers" },
            }
        );
        Assert.Contains("mute:second:True", rig.Calls);
        Assert.Contains("select:speakers:False", rig.Calls);
    }

    [Fact]
    public void FailedSelectionDoesNotUnmuteTargetOrBlockOtherCategory()
    {
        var rig = new Rig { Fail = call => call == "select:mic:False" };
        rig.Run(Pair());
        Assert.DoesNotContain("mute:mic:False", rig.Calls);
        Assert.Contains("select:speakers:False", rig.Calls);
    }

    [Fact]
    public void EnumerationFailureRecoversOnNextPress()
    {
        var rig = new Rig { UnavailableDirection = Direction.Recording };
        rig.Run(Pair());
        Assert.Contains("select:speakers:False", rig.Calls);
        Assert.DoesNotContain("select:mic:False", rig.Calls);
        rig.UnavailableDirection = null;
        rig.Run(Pair());
        Assert.Contains("select:mic:False", rig.Calls);
    }

    [Theory]
    [InlineData(HotkeyMuteAction.Mute)]
    [InlineData(HotkeyMuteAction.Unmute)]
    public void ExcludedTargetCanBeSelectedButNeverMutedOrUnmuted(HotkeyMuteAction action)
    {
        var rig = new Rig();
        rig.Settings.Devices.Add(new() { Id = "mic", ExcludeFromHotkeyMute = true });
        rig.Run(
            new()
            {
                Recording = new()
                {
                    DeviceId = "mic",
                    MuteAction = action,
                    MuteOthers = true,
                },
            }
        );
        Assert.Equal(new[] { "select:mic:False" }, rig.Calls);
    }

    [Theory]
    [InlineData(false, null, false)]
    [InlineData(true, null, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    public void CommunicationsOverrideFallsBackToGeneralSetting(
        bool general,
        bool? specific,
        bool expected
    )
    {
        var rig = new Rig();
        rig.Settings.AlsoCommunications = general;
        var hotkey = Pair();
        hotkey.AlsoCommunications = specific;
        rig.Run(hotkey);
        Assert.Contains($"select:speakers:{expected}", rig.Calls);
    }

    [Fact]
    public void UnexpectedProgrammingErrorsAreNotSilentlySwallowed()
    {
        var executor = new DeviceHotkeyExecutor(
            _ => throw new InvalidOperationException("bug"),
            (_, _) => { },
            (_, _) => { }
        );
        Assert.Throws<InvalidOperationException>(() => executor.Execute(Pair(), new()));
    }

    [Fact]
    public void OldSettingsGetSafeDefaultsAndNewSettingsRoundTripWithIndependentClones()
    {
        var old = JsonSerializer.Deserialize<AppSettings>(
            """{"Hotkeys":[{"Function":0,"VirtualKey":65}]}"""
        )!;
        old.Validate();
        Assert.Null(old.Hotkeys[0].Recording.DeviceId);
        Assert.Equal(HotkeyMuteAction.KeepCurrentState, old.Hotkeys[0].Recording.MuteAction);
        Assert.False(new DeviceSettings().ExcludeFromHotkeyMute);
        var settings = new AppSettings
        {
            Hotkeys = [Pair()],
            Devices = [new() { Id = "special", ExcludeFromHotkeyMute = true }],
        };
        settings.Hotkeys[0].Name = "Headset";
        settings.Hotkeys[0].VirtualKey = 65;
        settings.Hotkeys[0].AlsoCommunications = false;
        settings.Hotkeys[0].Recording.DeviceName = "Headset microphone";
        settings.Hotkeys[0].Recording.MuteOthers = true;
        var clone = settings.Clone();
        clone.Validate();
        Assert.True(clone.Devices[0].ExcludeFromHotkeyMute);
        Assert.Equal("Headset", clone.Hotkeys[0].Name);
        Assert.False(clone.Hotkeys[0].AlsoCommunications);
        Assert.Equal("Headset microphone", clone.Hotkeys[0].Recording.DeviceName);
        Assert.True(clone.Hotkeys[0].Recording.MuteOthers);
        Assert.Equal(HotkeyMuteAction.Unmute, clone.Hotkeys[0].Recording.MuteAction);
        var edit = clone.Hotkeys[0].Clone();
        edit.Recording.DeviceId = "different";
        Assert.Equal("mic", clone.Hotkeys[0].Recording.DeviceId);
    }

    [Theory]
    [InlineData("""{"Playback":null}""")]
    [InlineData("""{"Recording":null}""")]
    [InlineData("""{"Recording":{"MuteAction":999}}""")]
    [InlineData("""{"Playback":{"DeviceId":" "}}""")]
    public void InvalidDeviceSettingsAreRejected(string json)
    {
        var hotkey = JsonSerializer.Deserialize<HotkeySettings>(json)!;
        hotkey.VirtualKey = 65;
        Assert.Throws<System.IO.InvalidDataException>(() =>
            new AppSettings { Hotkeys = [hotkey] }.Validate()
        );
    }

    private static HotkeySettings Pair() =>
        new()
        {
            Function = HotkeyAction.SelectAudioDevices,
            Playback = new() { DeviceId = "speakers", MuteAction = HotkeyMuteAction.Unmute },
            Recording = new() { DeviceId = "mic", MuteAction = HotkeyMuteAction.Unmute },
        };

    private static AudioDevice Device(string id, Direction direction) =>
        new(id, id, id, "", direction, false, false, false);

    private sealed class Rig
    {
        public AppSettings Settings { get; } = new();
        public List<AudioDevice> Devices { get; } =
        [Device("mic", Direction.Recording), Device("speakers", Direction.Playback)];
        public List<string> Calls { get; } = [];
        public Func<string, bool>? Fail { get; set; }
        public Direction? UnavailableDirection { get; set; }

        public IReadOnlyList<string> Run(HotkeySettings hotkey) =>
            new DeviceHotkeyExecutor(
                direction =>
                    direction == UnavailableDirection
                        ? throw new COMException("Audio service restarting")
                        : Devices.Where(d => d.Direction == direction).ToArray(),
                (device, communications) => Record($"select:{device.Id}:{communications}"),
                (id, mute) => Record($"mute:{id}:{mute}")
            ).Execute(hotkey, Settings);

        private void Record(string call)
        {
            Calls.Add(call);
            if (Fail?.Invoke(call) == true)
            {
                throw new COMException("Device disconnected");
            }
        }
    }
}
