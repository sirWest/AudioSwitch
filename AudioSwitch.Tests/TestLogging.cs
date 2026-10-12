using System.IO;
using System.Runtime.CompilerServices;

namespace AudioSwitch.Tests;

internal static class TestLogging
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Child GUI/CLI processes inherit this directory and use their own log files.
        Environment.SetEnvironmentVariable(
            "AUDIOSWITCH_LOG_DIRECTORY",
            Path.Combine(AppContext.BaseDirectory, "test-logs", Guid.NewGuid().ToString("N"))
        );
    }
}
