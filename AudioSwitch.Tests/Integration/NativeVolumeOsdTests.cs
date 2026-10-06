using System.Runtime.ExceptionServices;
using AudioSwitch.Core.Audio;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class NativeVolumeOsdTests
{
    [SystemFact]
    public void ShowingNativeOsdRepeatedlyDoesNotChangeAudio()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var audio = new AudioService();
                var before = Enum.GetValues<Direction>()
                    .SelectMany(audio.List)
                    .ToDictionary(device => device.Id, device => audio.State(device.Id));
                Assert.NotEmpty(before);

                for (var i = 0; i < 3; i++)
                {
                    Assert.True(Native.TryShowVolumeOsd(), "Windows shell volume OSD unavailable.");
                }

                foreach (var (id, state) in before)
                {
                    Assert.Equal(state, audio.State(id));
                }
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
