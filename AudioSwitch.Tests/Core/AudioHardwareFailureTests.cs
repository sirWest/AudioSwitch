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
        Assert.Equal("Unknown", fallback.Name);
        Assert.Equal("Unknown", fallback.Description);
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

        public int Activate(ref Guid iid, uint context, nint parameters, out object instance)
        {
            instance = null!;
            return Fail;
        }

        public int OpenPropertyStore(uint access, out IPropertyStore store)
        {
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
            device = null!;
            return DefaultResult;
        }

        public int GetDevice(string id, out IMMDevice device)
        {
            device = null!;
            return Removed;
        }

        public int RegisterEndpointNotificationCallback(IMMNotificationClient client) => 0;

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
}
