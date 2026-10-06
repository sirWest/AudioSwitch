using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class AudioAvailabilityTests
{
    [Theory]
    [InlineData(unchecked((int)0x80070490))]
    [InlineData(unchecked((int)0x8007048F))]
    [InlineData(unchecked((int)0x88890004))]
    [InlineData(unchecked((int)0x88890010))]
    [InlineData(unchecked((int)0x88890026))]
    public void RecognizesUnavailableEndpointsThroughWindowsExceptionMapping(int result)
    {
        var exception = Assert.ThrowsAny<Exception>(() => Marshal.ThrowExceptionForHR(result));
        Assert.True(AudioUnavailableException.IsUnavailable(exception));
    }

    [Fact]
    public void UnrelatedFailuresAreNotTreatedAsMissingDevices()
    {
        Assert.False(
            AudioUnavailableException.IsUnavailable(
                new COMException("Access denied", unchecked((int)0x80070005))
            )
        );
        Assert.False(
            AudioUnavailableException.IsUnavailable(new ArgumentException("Invalid argument"))
        );
        Assert.False(
            AudioUnavailableException.IsUnavailable(new ObjectDisposedException("monitor"))
        );
        Assert.False(
            AudioUnavailableException.IsUnavailable(
                new InvalidOperationException("Unexpected failure")
            )
        );
        Assert.True(
            AudioUnavailableException.IsUnavailable(
                new AudioUnavailableException("No active devices")
            )
        );
    }

    [SystemFact]
    public void StaleIdsFailBeforeAnyAudioMutation()
    {
        using var audio = new AudioService();
        var id = "{0.0.0.00000000}.{" + Guid.NewGuid() + "}";
        var device = new AudioDevice(
            id,
            "Removed",
            "Removed",
            "",
            Direction.Playback,
            true,
            true,
            true
        );
        Action[] operations =
        [
            () => audio.SetDefault(id, AudioRole.Multimedia),
            () => audio.Select(device, new AppSettings()),
            () => audio.State(id),
            () => audio.SetVolume(id, .5f),
            () => audio.SetMute(id),
            () =>
            {
                using var monitor = audio.Monitor(id);
            },
        ];

        foreach (var operation in operations)
        {
            var exception = Assert.ThrowsAny<Exception>(operation);
            Assert.True(AudioUnavailableException.IsUnavailable(exception), exception.ToString());
        }
    }

    [SystemFact]
    public void CyclingWithEveryEndpointHiddenHandlesBothDirectionsWithoutChangingDefaults()
    {
        using var audio = new AudioService();
        foreach (var direction in Enum.GetValues<Direction>())
        {
            var original = audio.DefaultId(direction);
            var settings = new AppSettings();
            settings.Devices.AddRange(
                audio
                    .List(direction)
                    .Select(device => new DeviceSettings
                    {
                        Id = device.Id,
                        Direction = direction,
                        Hidden = true,
                    })
            );
            Assert.Throws<AudioUnavailableException>(() => audio.Cycle(direction, false, settings));
            Assert.Throws<AudioUnavailableException>(() => audio.Cycle(direction, true, settings));
            Assert.Equal(original, audio.DefaultId(direction));
        }
    }

    [SystemFact]
    public void StartupIgnoresSavedDevicesThatAreNoLongerPresent()
    {
        using var audio = new AudioService();
        var settings = new AppSettings();
        settings.Devices.Add(
            new DeviceSettings
            {
                Id = "AudioSwitch.Tests.Missing." + Guid.NewGuid(),
                StartupMultimedia = true,
                StartupCommunications = true,
            }
        );
        audio.ApplyStartup(settings);
    }
}
