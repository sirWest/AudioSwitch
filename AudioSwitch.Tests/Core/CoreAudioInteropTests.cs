using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.CoreAudio;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class CoreAudioInteropTests
{
    [Fact]
    public void NativeStructuresMatchWindowsLayouts()
    {
        Assert.Equal(nint.Size == 8 ? 24 : 16, Marshal.SizeOf<PropVariant>());
        Assert.Equal(20, Marshal.SizeOf<PropertyKey>());
        Assert.Equal(28, Marshal.SizeOf<VolumeNotification>());
        Assert.Equal(8, Marshal.OffsetOf<PropVariant>(nameof(PropVariant.Pointer)).ToInt32());
        Assert.Equal(
            20,
            Marshal.OffsetOf<VolumeNotification>(nameof(VolumeNotification.Volume)).ToInt32()
        );
    }

    [Fact]
    public void PropertyStringsAreReadAndNativeMemoryIsCleared()
    {
        var value = new PropVariant
        {
            Type = 31,
            Pointer = Marshal.StringToCoTaskMemUni("Headphones - USB Audio"),
        };
        try
        {
            Assert.Equal("Headphones - USB Audio", value.String);
        }
        finally
        {
            Assert.Equal(0, PropVariant.PropVariantClear(ref value));
        }

        Assert.Equal(0, value.Type);
        Assert.Null(value.String);
    }

    [Fact]
    public void VolumeCallbackReadsBoolAndScalarAndStopsAfterTeardown()
    {
        AudioState? received = null;
        var callback = new VolumeCallback(state => received = state);
        var memory = Marshal.AllocHGlobal(Marshal.SizeOf<VolumeNotification>());
        try
        {
            Marshal.StructureToPtr(
                new VolumeNotification
                {
                    Muted = 1,
                    Volume = .375f,
                    Channels = 2,
                },
                memory,
                false
            );
            Assert.Equal(0, callback.OnNotify(memory));
            Assert.Equal(new AudioState(.375f, true), received);
            callback.Stop();
            received = null;
            Assert.Equal(0, callback.OnNotify(memory));
            Assert.Null(received);
            Assert.NotEqual(0, callback.OnNotify(0));
            var throwing = new VolumeCallback(_ =>
                throw new InvalidOperationException("Subscriber failed")
            );
            Assert.Equal(0, throwing.OnNotify(memory));
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    [Fact]
    public void EndpointCallbacksHandleMissingDefaultAndSubscriberFailures()
    {
        AudioChange? received = null;
        var callback = new EndpointNotifications(change => received = change);
        Assert.Equal(
            0,
            callback.OnDefaultDeviceChanged(DataFlow.Capture, Role.Communications, null)
        );
        Assert.Equal(
            new AudioChange(null, Direction.Recording, AudioRole.Communications),
            received
        );
        callback.Stop();
        received = null;
        Assert.Equal(0, callback.OnDeviceRemoved("removed"));
        Assert.Null(received);
        var throwing = new EndpointNotifications(_ =>
            throw new InvalidOperationException("Subscriber failed")
        );
        Assert.Equal(0, throwing.OnDeviceAdded("added"));
    }
}
