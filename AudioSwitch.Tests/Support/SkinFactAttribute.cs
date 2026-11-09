using System.Runtime.CompilerServices;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class SkinFactAttribute : FactAttribute
{
    public SkinFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0
    )
        : base(sourceFilePath, sourceLineNumber)
    {
        if (Environment.GetEnvironmentVariable("AUDIOSWITCH_TEST_SKINS") != "1")
        {
            Skip = "Set AUDIOSWITCH_TEST_SKINS=1 to test the additional OSD skins.";
        }
    }
}
