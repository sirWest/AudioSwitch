using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class DeviceNameTests
{
    [Theory]
    [InlineData(true, "Alpha system", "Zulu system")]
    [InlineData(false, "Alpha description", "Zulu description")]
    public void ListsSortByVisibleNamesAndUseIdsToBreakTies(
        bool hardware,
        string first,
        string last
    )
    {
        var settings = new AppSettings { ShowHardwareName = hardware };
        settings.Devices.Add(
            new()
            {
                Id = "renamed",
                UseCustomName = true,
                CustomName = "middle",
            }
        );
        settings.Devices.Add(
            new()
            {
                Id = "disabled",
                UseCustomName = false,
                CustomName = "AAA",
            }
        );
        settings.Devices.Add(
            new()
            {
                Id = "empty",
                UseCustomName = true,
                CustomName = "",
            }
        );
        AudioDevice[] devices =
        [
            new("renamed", "ZZZ", "ZZZ", "", Direction.Playback, false, false, false),
            new(
                "empty",
                "Zulu system",
                "Zulu description",
                "",
                Direction.Playback,
                false,
                false,
                false
            ),
            new(
                "disabled",
                "Alpha system",
                "Alpha description",
                "",
                Direction.Playback,
                false,
                false,
                false
            ),
            new("tie", "MIDDLE", "MIDDLE", "", Direction.Playback, false, false, false),
        ];

        var sorted = AudioDevice.SortByDisplayName(devices, settings);

        Assert.Equal(new[] { "disabled", "renamed", "tie", "empty" }, sorted.Select(d => d.Id));
        Assert.Equal(
            new[] { first, "middle", "MIDDLE", last },
            sorted.Select(d => d.DisplayName(settings))
        );
    }
}
