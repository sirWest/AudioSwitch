using System.Runtime.CompilerServices;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class UiFactAttribute : FactAttribute
{
    public UiFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0
    )
        : base(sourceFilePath, sourceLineNumber)
    {
        if (Environment.GetEnvironmentVariable("AUDIOSWITCH_TEST_UI") != "1")
        {
            Skip = "Set AUDIOSWITCH_TEST_UI=1 in an interactive desktop session.";
        }
    }
}
