using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.CoreAudio;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class AudioHardwareFailureTests
{
    private const int Fail = unchecked((int)0x80004005);
    private const int Removed = unchecked((int)0x80070490);

    [Theory]
    [InlineData(unchecked((int)0x80070057))] // #135: invalid metering argument
    [InlineData(unchecked((int)0x80004002))] // #83: unsupported interface
    [InlineData(unchecked((int)0x80070002))] // #147: endpoint activation
    [InlineData(unchecked((int)0xE000020B))] // #133/#156: property store
    [InlineData(unchecked((int)0x8007001F))] // #146/#130: driver I/O failure
    [InlineData(unchecked((int)0x80070006))] // invalid handle
    [InlineData(Fail)] // #157/#168: default endpoint operations
    [InlineData(unchecked((int)0x81234567))] // unknown vendor failure
    public void NativeFailuresKeepTheirCodeAndCause(int result)
    {
        var error = Assert.Throws<AudioOperationException>(() =>
            AudioOperationException.Check(result, "set the Multimedia default device")
        );
        Assert.Equal(result, error.HResult);
        Assert.Equal(result, error.InnerException!.HResult);
        Assert.Contains("Multimedia", error.Message);
        Assert.True(AudioOperationException.IsDeviceFailure(error));
        Assert.False(AudioOperationException.IsDeviceFailure(new ArgumentException()));
        Assert.False(AudioOperationException.IsDeviceFailure(new ObjectDisposedException("test")));
    }

    [Fact]
    public void MeterFailuresAreThrottledAndRecoverWithoutReplacingTheVolumeMonitor()
    {
        var clock = new TestClock();
        var reads = 0;
        var resets = 0;
        var broken = true;
        var reader = new PeakReader(
            () =>
            {
                reads++;
                if (broken)
                {
                    AudioOperationException.Check(unchecked((int)0x80070057));
                }
                return new[] { .25f, .75f };
            },
            () => resets++,
            clock
        );

        for (var tick = 0; tick < 100; tick++)
        {
            Assert.Equal((0f, 0f), reader.Read());
        }
        Assert.Equal(1, reads);
        Assert.Equal(1, resets);
        broken = false;
        clock.Now += TimeSpan.FromSeconds(5);
        Assert.Equal((.25f, .75f), reader.Read());
        Assert.Equal(2, reads);
        Assert.Equal((.25f, .75f), reader.Read());
        Assert.Equal(3, reads);
    }

    [Fact]
    public void UnsupportedMeterInterfacesAreContainedButRemovalRequestsMonitorRecovery()
    {
        var unsupported = new PeakReader(() => throw new InvalidCastException(), () => { });
        Assert.Equal((0f, 0f), unsupported.Read());
        var removed = new PeakReader(
            () =>
            {
                AudioOperationException.Check(Removed);
                return Array.Empty<float>();
            },
            () => { }
        );
        var error = Assert.Throws<AudioOperationException>(() => removed.Read());
        Assert.True(AudioUnavailableException.IsUnavailable(error));
        var bug = new PeakReader(() => throw new NullReferenceException(), () => { });
        Assert.Throws<NullReferenceException>(() => bug.Read());
    }

    [Fact]
    public void MeterValidatesNativeChannelCountsAndSamples()
    {
        var native = new TestMeter { Count = uint.MaxValue };
        using var meter = new AudioMeterInformation(native);
        Assert.Throws<AudioOperationException>(() => meter.PeakValues);
        Assert.Equal(0, native.PeakReads);
        native.Count = 0;
        Assert.Empty(meter.PeakValues);
        Assert.Equal(0, native.PeakReads);
        native.Count = 5;
        native.Values = [float.NaN, float.PositiveInfinity, -.5f, 2f, .5f];
        Assert.Equal(new[] { 0f, 0f, 0f, 1f, .5f }, meter.PeakValues);
        native.Result = unchecked((int)0x80070057);
        Assert.Throws<AudioOperationException>(() => meter.PeakValues);
    }

    [Fact]
    public void MonoAndEmptyMeterResultsAreSupported()
    {
        Assert.Equal((.5f, .5f), new PeakReader(() => new[] { .5f }, () => { }).Read());
        Assert.Equal((0f, 0f), new PeakReader(() => Array.Empty<float>(), () => { }).Read());
    }

    [Fact]
    public void BadPropertiesUseFallbacksAndDoNotDropHealthyEndpoints()
    {
        var native = new TestEnumerator();
        native.Devices.Add(
            new TestDevice("broken-properties") { PropertyResult = unchecked((int)0xE000020B) }
        );
        native.Devices.Add(new TestDevice("healthy"));
        native.Devices.Add(new TestDevice("removed") { StateResult = Removed });
        native.Devices.Add(new TestDevice("bad-id") { IdResult = Fail });
        native.Devices.Add(new TestDevice("collection-race"));
        native.FailedIndex = 4;
        using var audio = new AudioService(new MMDeviceEnumerator(native));
        var devices = audio.List(Direction.Playback);
        Assert.Equal(2, devices.Count);
        var fallback = Assert.Single(devices, d => d.Id == "broken-properties");
        Assert.Equal("<Unknown name>", fallback.Name);
        Assert.Equal("<Unknown name>", fallback.Description);
        Assert.Equal("", fallback.IconPath);
        Assert.Contains(devices, d => d.Id == "healthy" && d.Name == "Test endpoint");
    }

    [Fact]
    public void DefaultLookupFailureDoesNotPreventEnumeration()
    {
        var native = new TestEnumerator { DefaultResult = Fail };
        native.Devices.Add(new TestDevice("healthy"));
        using var audio = new AudioService(new MMDeviceEnumerator(native));
        Assert.Null(audio.DefaultId(Direction.Playback));
        var device = Assert.Single(audio.List(Direction.Playback));
        Assert.False(device.Multimedia);
        Assert.False(device.Communications);
        Assert.False(device.Console);
    }

    [Fact]
    public void FailedPropertyValueDoesNotDiscardOtherMetadata()
    {
        using var device = new MMDevice(new TestDevice("partial") { IconResult = Fail });
        Assert.Equal("Test endpoint", device.FriendlyName);
        Assert.Equal("Test endpoint", device.DeviceFriendlyName);
        Assert.Equal("", device.IconPath);
    }

    [Fact]
    public void RemovedDeviceCannotReachTheDefaultDevicePolicy()
    {
        using var audio = new AudioService(new MMDeviceEnumerator(new TestEnumerator()));
        var error = Assert.Throws<AudioOperationException>(() =>
            audio.SetDefault("removed", AudioRole.Multimedia)
        );
        Assert.True(AudioUnavailableException.IsUnavailable(error));
    }

    [Fact]
    public void PropertiesAreCachedAcrossWrappersAndInvalidatedByDeviceChangesOnly()
    {
        var native = new TestEnumerator();
        var endpoint = new TestDevice("device") { PropertyResult = unchecked((int)0x80004002) };
        native.Devices.Add(endpoint);
        using var audio = new AudioService(new MMDeviceEnumerator(native));
        Assert.Equal("<Unknown name>", Assert.Single(audio.List(Direction.Playback)).Name);
        var reads = endpoint.PropertyReads;
        endpoint.PropertyResult = 0;
        native.Callback!.OnDefaultDeviceChanged(DataFlow.Render, Role.Multimedia, "device");
        Assert.Equal("<Unknown name>", Assert.Single(audio.List(Direction.Playback)).Name);
        Assert.Equal(reads, endpoint.PropertyReads);
        native.Callback.OnPropertyValueChanged("device", default);
        Assert.Equal("Test endpoint", Assert.Single(audio.List(Direction.Playback)).Name);
        Assert.True(endpoint.PropertyReads > reads);
    }

    [Fact]
    public void ActivationFailureRecoversOnSameIdAfterDeviceNotification()
    {
        var native = new TestEnumerator();
        var endpoint = new TestDevice("device") { ActivateResult = unchecked((int)0x80070002) };
        native.Devices.Add(endpoint);
        using var audio = new AudioService(new MMDeviceEnumerator(native));
        Assert.Throws<AudioOperationException>(() => audio.Monitor("device"));
        endpoint.ActivateResult = 0;
        // No immediate retry storm while the endpoint is still in its cooldown.
        Assert.Throws<AudioOperationException>(() => audio.Monitor("device"));
        Assert.Equal(1, endpoint.Activations);
        native.Callback!.OnDeviceStateChanged("device", DeviceState.Active);
        using var monitor = audio.Monitor("device");
        Assert.Equal(.5f, monitor.State.Volume);
        Assert.Equal(2, endpoint.Activations);
    }

    [Theory]
    [InlineData(Fail)]
    [InlineData(0)]
    public void FailedOrUnconfirmedSwitchDoesNotChangeReportedDefaultOrReplayWrite(int result)
    {
        var native = new TestEnumerator { DefaultResult = 0 };
        native.Devices.Add(new TestDevice("old"));
        native.Devices.Add(new TestDevice("new"));
        native.Defaults[(DataFlow.Render, Role.Multimedia)] = "old";
        var writes = 0;
        using var audio = new AudioService(
            new MMDeviceEnumerator(native),
            (_, _) =>
            {
                writes++;
                return result;
            }
        );
        Assert.Throws<AudioOperationException>(() => audio.SetDefault("new", AudioRole.Multimedia));
        Assert.Equal(1, writes);
        Assert.Equal("old", Assert.Single(audio.List(Direction.Playback), d => d.Multimedia).Id);
    }

    [Fact]
    public void PartialRoleSwitchRetainsActualRoles()
    {
        var native = new TestEnumerator { DefaultResult = 0 };
        native.Devices.Add(new TestDevice("old"));
        native.Devices.Add(new TestDevice("new"));
        native.Defaults[(DataFlow.Render, Role.Multimedia)] = "old";
        native.Defaults[(DataFlow.Render, Role.Communications)] = "old";
        using var audio = new AudioService(
            new MMDeviceEnumerator(native),
            (id, role) =>
            {
                if (role == AudioRole.Communications)
                    return Fail;
                native.Defaults[(DataFlow.Render, (Role)role)] = id;
                return 0;
            }
        );
        Assert.Throws<AudioOperationException>(() =>
            audio.SetDefault("new", AudioRole.Multimedia, AudioRole.Communications)
        );
        Assert.Equal("new", audio.DefaultId(Direction.Playback));
        Assert.Equal("old", audio.DefaultId(Direction.Playback, AudioRole.Communications));
    }

    [Fact]
    public void ReadBackFailureDoesNotReplaySuccessfulVolumeWrite()
    {
        var native = new TestEnumerator();
        var endpoint = new TestDevice("device") { ActivateResult = 0 };
        native.Devices.Add(endpoint);
        endpoint.Volume.ReadResult = unchecked((int)0x80070006);
        using var audio = new AudioService(new MMDeviceEnumerator(native));
        var error = Assert.Throws<AudioOperationException>(() => audio.SetVolume("device", .8f));
        Assert.Equal("read volume scalar", error.Operation);
        Assert.Equal(.8f, endpoint.Volume.Value);
        Assert.Equal(1, endpoint.Volume.Writes);
        endpoint.Volume.ReadResult = 0;
        Assert.Equal(.8f, audio.State("device").Volume);
    }

    private sealed class TestClock : TimeProvider
    {
        internal DateTimeOffset Now = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class TestMeter : IAudioMeterInformation
    {
        internal uint Count;
        internal int Result;
        internal int PeakReads;
        internal float[] Values = [];

        public int GetPeakValue(out float peak)
        {
            peak = 0;
            return 0;
        }

        public int GetMeteringChannelCount(out uint count)
        {
            count = Count;
            return 0;
        }

        public int GetChannelsPeakValues(uint count, float[] peaks)
        {
            PeakReads++;
            Values.CopyTo(peaks, 0);
            return Result;
        }

        public int QueryHardwareSupport(out uint mask)
        {
            mask = 0;
            return 0;
        }
    }

    private sealed class TestDevice(string id) : IMMDevice, IPropertyStore
    {
        internal int PropertyResult;
        internal int IconResult;
        internal int StateResult;
        internal int IdResult;
        internal int PropertyReads;
        internal int ActivateResult = Fail;
        internal int Activations;
        internal TestVolume Volume = new();

        public int Activate(ref Guid iid, uint context, nint parameters, out object instance)
        {
            Activations++;
            instance = Volume;
            return ActivateResult;
        }

        public int OpenPropertyStore(uint access, out IPropertyStore store)
        {
            PropertyReads++;
            store = this;
            return PropertyResult;
        }

        public int GetId(out string value)
        {
            value = id;
            return IdResult;
        }

        public int GetState(out DeviceState state)
        {
            state = DeviceState.Active;
            return StateResult;
        }

        public int GetCount(out uint count)
        {
            count = 0;
            return 0;
        }

        public int GetAt(uint index, out PropertyKey key)
        {
            key = default;
            return 0;
        }

        public int GetValue(ref PropertyKey key, out PropVariant value)
        {
            value = default;
            if (key.Id == 12 && IconResult != 0)
                return IconResult;
            value = new PropVariant
            {
                Type = 31,
                Pointer = Marshal.StringToCoTaskMemUni("Test endpoint"),
            };
            return 0;
        }

        public int SetValue(ref PropertyKey key, ref PropVariant value) => 0;

        public int Commit() => 0;
    }

    private sealed class TestEnumerator : IMMDeviceEnumerator, IMMDeviceCollection
    {
        internal readonly List<TestDevice> Devices = [];
        internal int DefaultResult = Removed;
        internal int FailedIndex = -1;
        internal IMMNotificationClient? Callback;
        internal Dictionary<(DataFlow, Role), string> Defaults = new();

        public int EnumAudioEndpoints(
            DataFlow flow,
            DeviceState mask,
            out IMMDeviceCollection devices
        )
        {
            devices = this;
            return 0;
        }

        public int GetDefaultAudioEndpoint(DataFlow flow, Role role, out IMMDevice device)
        {
            device = Defaults.TryGetValue((flow, role), out var id)
                ? Devices.FirstOrDefault(d =>
                {
                    d.GetId(out var value);
                    return value == id;
                })!
                : null!;
            return DefaultResult != 0 ? DefaultResult
                : device is null ? Removed
                : 0;
        }

        public int GetDevice(string id, out IMMDevice device)
        {
            device = Devices.FirstOrDefault(d =>
            {
                d.GetId(out var value);
                return value == id;
            })!;
            return device is null ? Removed : 0;
        }

        public int RegisterEndpointNotificationCallback(IMMNotificationClient client)
        {
            Callback = client;
            return 0;
        }

        public int UnregisterEndpointNotificationCallback(IMMNotificationClient client) => 0;

        public int GetCount(out uint count)
        {
            count = (uint)Devices.Count;
            return 0;
        }

        public int Item(uint index, out IMMDevice device)
        {
            device = Devices[(int)index];
            return index == FailedIndex ? Fail : 0;
        }
    }

    private sealed class TestVolume : IAudioEndpointVolume
    {
        internal float Value = .5f;
        internal int ReadResult;
        internal int Writes;
        private bool muted;

        public int RegisterControlChangeNotify(IAudioEndpointVolumeCallback callback) => 0;

        public int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback callback) => 0;

        public int GetChannelCount(out uint count)
        {
            count = 2;
            return 0;
        }

        public int SetMasterVolumeLevel(float value, ref Guid context) => 0;

        public int SetMasterVolumeLevelScalar(float value, ref Guid context)
        {
            Value = value;
            Writes++;
            return 0;
        }

        public int GetMasterVolumeLevel(out float value)
        {
            value = Value;
            return 0;
        }

        public int GetMasterVolumeLevelScalar(out float value)
        {
            value = Value;
            return ReadResult;
        }

        public int SetChannelVolumeLevel(uint channel, float value, ref Guid context) => 0;

        public int SetChannelVolumeLevelScalar(uint channel, float value, ref Guid context) => 0;

        public int GetChannelVolumeLevel(uint channel, out float value)
        {
            value = Value;
            return 0;
        }

        public int GetChannelVolumeLevelScalar(uint channel, out float value)
        {
            value = Value;
            return 0;
        }

        public int SetMute(bool value, ref Guid context)
        {
            muted = value;
            return 0;
        }

        public int GetMute(out bool value)
        {
            value = muted;
            return 0;
        }

        public int GetVolumeStepInfo(out uint step, out uint count)
        {
            step = 0;
            count = 100;
            return 0;
        }

        public int VolumeStepUp(ref Guid context) => 0;

        public int VolumeStepDown(ref Guid context) => 0;

        public int QueryHardwareSupport(out uint mask)
        {
            mask = 0;
            return 0;
        }

        public int GetVolumeRange(out float min, out float max, out float increment)
        {
            min = 0;
            max = 1;
            increment = .01f;
            return 0;
        }
    }
}
