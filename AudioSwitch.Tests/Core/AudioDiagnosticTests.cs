using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using AudioSwitch.Core.Audio;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class AudioDiagnosticTests
{
    [Fact]
    public void FailedEnrichmentAndReentrantLoggingPreserveTheOriginalError()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log");
        try
        {
            var log = new AudioDiagnosticLog(path);
            log.Failure(
                "read device",
                new COMException("original", unchecked((int)0x81234567)),
                "endpoint",
                () =>
                {
                    log.Failure("recursive", new Exception("must not recurse"));
                    throw new COMException("metadata failed", unchecked((int)0x80070002));
                }
            );
            var line = Assert.Single(File.ReadAllLines(path));
            using var record = JsonDocument.Parse(line);
            Assert.Equal("0x81234567", record.RootElement.GetProperty("hresult").GetString());
            Assert.Contains("original", record.RootElement.GetProperty("exception").GetString());
            Assert.Contains(
                "metadata failed",
                record.RootElement.GetProperty("diagnosticCollectionError").GetString()
            );
            Assert.Equal("endpoint", record.RootElement.GetProperty("deviceId").GetString());
            Assert.False(
                string.IsNullOrWhiteSpace(record.RootElement.GetProperty("osVersion").GetString())
            );
            Assert.True(record.RootElement.TryGetProperty("appVersion", out _));
            Assert.True(record.RootElement.TryGetProperty("thread", out _));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void UnwritableLogDoesNotThrow()
    {
        var blocker = Path.GetTempFileName();
        try
        {
            var log = new AudioDiagnosticLog(Path.Combine(blocker, "errors.log"));
            log.Failure("operation", new Exception("failure"));
            log.Recovered("operation");
        }
        finally
        {
            File.Delete(blocker);
        }
    }

    [Fact]
    public void RepeatedErrorsAreSummarizedAndLogsRotate()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".log");
        var clock = new Clock();
        try
        {
            var log = new AudioDiagnosticLog(path, clock, 1);
            var error = new COMException("failed", unchecked((int)0x80004005));
            log.Failure("write volume", error, "device");
            for (var i = 0; i < 100; i++)
                log.Failure("write volume", error, "device");
            Assert.Single(File.ReadAllLines(path));
            clock.Now += TimeSpan.FromMinutes(1);
            log.Failure("write volume", error, "device");
            Assert.True(File.Exists(path + ".1"));
            using var record = JsonDocument.Parse(Assert.Single(File.ReadAllLines(path)));
            Assert.Equal(
                "100",
                record.RootElement.GetProperty("suppressedSincePrevious").GetString()
            );
            log.Recovered("write volume", "device");
            using var recovered = JsonDocument.Parse(Assert.Single(File.ReadAllLines(path)));
            Assert.Equal("recovered", recovered.RootElement.GetProperty("event").GetString());
        }
        finally
        {
            File.Delete(path);
            File.Delete(path + ".1");
        }
    }

    [Fact]
    public void UnsupportedMeterIsRememberedAcrossMonitorsUntilDeviceChanges()
    {
        var clock = new Clock();
        var session = new DeviceSession("headset", clock);
        var calls = 0;
        var unsupported = true;
        float[] Read()
        {
            calls++;
            if (unsupported)
                throw new InvalidCastException("meter unsupported");
            return [.2f];
        }
        var first = new PeakReader(Read, () => { }, clock, session);
        Assert.Equal((0f, 0f), first.Read());
        clock.Now += TimeSpan.FromDays(1);
        var second = new PeakReader(Read, () => { }, clock, session);
        Assert.Equal((0f, 0f), second.Read());
        Assert.Equal(1, calls);
        session.Reset();
        unsupported = false;
        Assert.Equal((.2f, .2f), second.Read());
        Assert.Equal(2, calls);
    }

    [Fact]
    public void SilenceIsNotAnUnsupportedMeter()
    {
        var calls = 0;
        var reader = new PeakReader(
            () =>
            {
                calls++;
                return new[] { calls == 1 ? 0f : .8f };
            },
            () => { }
        );
        Assert.Equal((0f, 0f), reader.Read());
        Assert.Equal((.8f, .8f), reader.Read());
    }

    [Theory]
    [InlineData(unchecked((int)0x80010108))]
    [InlineData(unchecked((int)0x800706BA))]
    public void DisconnectedProxyRequiresMonitorRecreation(int code)
    {
        var reader = new PeakReader(() => throw new COMException("disconnected", code), () => { });
        var error = Assert.Throws<COMException>(() => reader.Read());
        Assert.True(AudioUnavailableException.IsUnavailable(error));
    }

    [Fact]
    public void UnknownPropertyFailureIsRetriedAfterCooldown()
    {
        var clock = new Clock();
        var session = new DeviceSession("endpoint", clock);
        var error = new COMException("unknown", unchecked((int)0x81234567));
        session.Failed("property", error);
        Assert.Same(error, session.Blocked("property"));
        Assert.Contains("0x81234567", session.Snapshot()["property"]);
        clock.Now += TimeSpan.FromSeconds(30);
        Assert.Null(session.Blocked("property"));
        session.Property("property", "name");
        Assert.True(session.TryProperty("property", out var value));
        Assert.Equal("name", value);
    }

    [Fact]
    public void PairFailuresAreReportedAndClearedForTheNextExecution()
    {
        var fail = true;
        var executor = new DeviceHotkeyExecutor(
            direction =>
                [
                    new AudioDevice(
                        "endpoint",
                        "name",
                        "description",
                        "",
                        direction,
                        false,
                        false,
                        false
                    ),
                ],
            (_, _) =>
            {
                if (fail)
                    throw new COMException("selection failed");
            },
            (_, _) => { }
        );
        var hotkey = new AudioSwitch.Core.Settings.HotkeySettings
        {
            Playback = new() { DeviceId = "endpoint" },
        };
        executor.Execute(hotkey, new());
        Assert.Single(executor.Failures);
        fail = false;
        executor.Execute(hotkey, new());
        Assert.Empty(executor.Failures);
    }

    private sealed class Clock : TimeProvider
    {
        internal DateTimeOffset Now = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
