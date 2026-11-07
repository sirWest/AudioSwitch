using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows.Threading;

namespace AudioSwitch;

public sealed partial class App
{
    private static readonly HttpClient UpdateClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private DispatcherTimer? updateTimer;
    private CancellationTokenSource? updateCancellation;
    private string? updateUrl;
    private bool updateNotified;

    private void ConfigureUpdateChecks()
    {
        if (!Settings.CheckForUpdates)
        {
            updateTimer?.Stop();
            updateTimer = null;
            updateCancellation?.Cancel();
            updateCancellation?.Dispose();
            updateCancellation = null;
            updateUrl = null;
            return;
        }
        if (updateTimer is not null)
            return;
        updateCancellation = new CancellationTokenSource();
        var cancellationToken = updateCancellation.Token;
        updateTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(24) };
        updateTimer.Tick += (_, _) => _ = CheckUpdatesAsync(cancellationToken);
        updateTimer.Start();
        _ = CheckUpdatesAsync(cancellationToken);
    }

    private async Task CheckUpdatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.github.com/repos/sirWest/AudioSwitch/releases/latest"
            );
            request.Headers.UserAgent.ParseAdd("AudioSwitch-update-check");
            using var response = await UpdateClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken)
            );
            var tag = json.RootElement.GetProperty("tag_name").GetString();
            var url = json.RootElement.GetProperty("html_url").GetString();
            var current = Assembly.GetExecutingAssembly().GetName().Version;
            if (cancellationToken.IsCancellationRequested || !Settings.CheckForUpdates)
                return;
            if (
                tag is null
                || !Version.TryParse(tag.TrimStart('v', 'V'), out var available)
                || current is null
                || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
                || !uri.AbsolutePath.StartsWith(
                    "/sirWest/AudioSwitch/releases/",
                    StringComparison.OrdinalIgnoreCase
                )
            )
                return;
            var normalized = new Version(
                available.Major,
                available.Minor,
                Math.Max(0, available.Build),
                Math.Max(0, available.Revision)
            );
            if (normalized <= current)
            {
                updateUrl = null;
                return;
            }
            updateUrl = uri.AbsoluteUri;
            if (!updateNotified)
            {
                updateNotified = true;
                tray.Notify(
                    $"AudioSwitch {tag} is available. Click to view the release.",
                    OpenUpdate
                );
            }
        }
        catch (Exception ex)
            when (ex
                    is HttpRequestException
                        or TaskCanceledException
                        or JsonException
                        or InvalidOperationException
                        or KeyNotFoundException
            )
        {
            // Update checks are optional; retry at the next scheduled interval.
        }
    }

    private void OpenUpdate()
    {
        if (Settings.CheckForUpdates && updateUrl is not null)
            Process.Start(new ProcessStartInfo(updateUrl) { UseShellExecute = true });
    }
}
