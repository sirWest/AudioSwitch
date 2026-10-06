using System.Runtime.CompilerServices;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class LocalAudioFactAttribute : FactAttribute
{
    public LocalAudioFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0
    )
        : base(sourceFilePath, sourceLineNumber)
    {
        if (Environment.GetEnvironmentVariable("AUDIOSWITCH_TEST_SWITCHING") != "1")
        {
            Skip =
                "Set AUDIOSWITCH_TEST_SWITCHING=1 in a local Windows session with switchable audio endpoints.";
        }
    }
}
