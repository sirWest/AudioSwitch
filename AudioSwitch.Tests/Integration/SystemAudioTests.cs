using AudioSwitch.Core.Audio;
using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace AudioSwitch.Tests;

public sealed class SystemAudioTests
{
    [SystemFact]
    public void RepeatedEnumerationAndMonitoringReleaseTheirResources()
    {
        for (var i = 0; i < 30; i++)
        {
            using var audio = new AudioService();
            foreach (var device in audio.List(Direction.Playback))
            {
                using var monitor = audio.Monitor(device.Id);
                Assert.InRange(monitor.State.Volume, 0, 1);
                var peaks = monitor.Peaks();
                Assert.InRange(peaks.Left, 0, 1);
                Assert.InRange(peaks.Right, 0, 1);
                monitor.Dispose();
                Assert.Throws<ObjectDisposedException>(() => monitor.Peaks());
            }

            audio.Dispose();
            Assert.Throws<ObjectDisposedException>(() => audio.List(Direction.Playback));
        }
    }

    [LocalAudioFact]
    public void ReappliesTheCurrentDefaultThroughWindowsPolicyInterface()
    {
        using var audio = new AudioService();
        var id =
            audio.DefaultId(Direction.Playback)
            ?? throw new InvalidOperationException("No playback endpoint.");
        audio.SetDefault(id, AudioRole.Multimedia);
        Assert.Equal(id, audio.DefaultId(Direction.Playback));
    }

    [SystemFact]
    public void EnumeratesRealEndpointsAndReadsTheirVolume()
    {
        using var audio = new AudioService();
        foreach (var direction in Enum.GetValues<Direction>())
        {
            var devices = audio.List(direction);
            Assert.Equal(devices.Count, devices.Select(d => d.Id).Distinct().Count());
            foreach (var device in devices)
            {
                Assert.NotEmpty(device.Name);
                Assert.InRange(audio.State(device.Id).Volume, 0, 1);
            }

            var id = audio.DefaultId(direction);
            if (id is not null)
            {
                Assert.Contains(devices, d => d.Id == id);
            }
        }
    }

    [SystemFact(true)]
    public void VolumeAndMuteRoundTripAndDeliverActualCallbacks()
    {
        using var audio = new AudioService();
        var id =
            audio.DefaultId(Direction.Playback)
            ?? throw new InvalidOperationException(
                "Connect a playback device before running this test."
            );
        var original = audio.State(id);
        using var monitor = audio.Monitor(id);
        using var changed = new ManualResetEventSlim();
        monitor.Changed += _ => changed.Set();
        try
        {
            var target = original.Volume > .1f ? original.Volume - .02f : original.Volume + .02f;
            audio.SetVolume(id, target);
            Assert.True(
                changed.Wait(TimeSpan.FromSeconds(5)),
                "No Windows volume callback received."
            );
            Assert.InRange(Math.Abs(audio.State(id).Volume - target), 0, .015);
            audio.SetMute(id, !original.Muted);
            Assert.Equal(!original.Muted, audio.State(id).Muted);
        }
        finally
        {
            try
            {
                audio.SetVolume(id, original.Volume);
            }
            finally
            {
                audio.SetMute(id, original.Muted);
            }
        }
    }

    [LocalAudioFact]
    public void SwitchesRealDefaultEndpointAndRestoresEveryRole()
    {
        using var audio = new AudioService();
        var devices = audio.List(Direction.Playback);
        Assert.True(
            devices.Count >= 2,
            "Connect at least two playback endpoints for the switching test."
        );
        var originals = Enum.GetValues<AudioRole>()
            .ToDictionary(role => role, role => audio.DefaultId(Direction.Playback, role));
        var target = devices.First(d => d.Id != originals[AudioRole.Multimedia]);
        using var changed = new ManualResetEventSlim();
        audio.Changed += change =>
        {
            if (
                change.Direction == Direction.Playback
                && change.Role == AudioRole.Multimedia
                && change.Id == target.Id
            )
            {
                changed.Set();
            }
        };
        try
        {
            audio.SetDefault(target.Id, AudioRole.Multimedia);
            Assert.True(
                changed.Wait(TimeSpan.FromSeconds(5)),
                "No Windows default-device callback received."
            );
            Assert.Equal(target.Id, audio.DefaultId(Direction.Playback));
        }
        finally
        {
            foreach (var (role, id) in originals)
            {
                if (id is not null)
                {
                    audio.SetDefault(id, role);
                }
            }
        }
    }
}
