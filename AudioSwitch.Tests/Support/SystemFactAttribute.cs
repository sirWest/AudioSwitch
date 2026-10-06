using System.Runtime.CompilerServices;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class SystemFactAttribute : FactAttribute
{
    public SystemFactAttribute(
        bool changesAudio = false,
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0
    )
        : base(sourceFilePath, sourceLineNumber)
    {
        var name = changesAudio ? "AUDIOSWITCH_TEST_MUTATIONS" : "AUDIOSWITCH_TEST_SYSTEM";
        if (Environment.GetEnvironmentVariable(name) != "1")
        {
            Skip = $"Set {name}=1 to run against the current Windows audio session.";
        }
    }
}
