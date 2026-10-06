using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class CliTests
{
    private static (int Code, string Output, string Error) Run(params string[] args)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AudioSwitch.sln")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        var exe = Path.Combine(
            root.FullName,
            "AudioSwitch.Cli",
            "bin",
            configuration,
            "net10.0-windows",
            "audioswitch-cli.exe"
        );
        var start = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(10000))
        {
            process.Kill();
            throw new TimeoutException("CLI did not exit.");
        }

        return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
    }

    [Fact]
    public void HelpWritesToRedirectedStdoutAndExitsSuccessfully()
    {
        var result = Run("--help");
        Assert.Equal(0, result.Code);
        Assert.Contains("--json", result.Output);
        Assert.Empty(result.Error);
    }

    [SystemFact]
    public void JsonListIsMachineReadableAndMissingLegacyArgumentIsAnError()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "AudioSwitch.CliTests",
            Guid.NewGuid().ToString("N"),
            "settings.json"
        );
        var result = Run("list", "--output", "--json", "--settings", path);
        Assert.Equal(0, result.Code);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        result = Run("/s", "--settings", path);
        Assert.Equal(2, result.Code);
        Assert.Empty(result.Output);
        Assert.Contains("Missing value", result.Error);
        Assert.False(File.Exists(path));
    }
}
