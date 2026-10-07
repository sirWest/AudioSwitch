using System.Windows.Threading;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Flyout;

internal sealed partial class FlyoutWindow
{
    private readonly DispatcherTimer meterTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(50),
    };

    private readonly DispatcherTimer recoveryTimer = new() { Interval = TimeSpan.FromSeconds(2) };

    private EndpointMonitor? monitor;
    private EndpointMonitor? trayMonitor;
    private AudioDevice? trayDevice;
    private AudioState? trayState;
    private AudioDevice? defaultDevice;

    private bool updatingVolume;
    private bool audioReadFailed;
    private bool reportedAudioFailure;

    private IReadOnlyList<AudioDevice> ReadDevices(Direction selected)
    {
        try
        {
            return app.Audio.List(selected, app.Settings);
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioReadFailed(ex);
            return Array.Empty<AudioDevice>();
        }
    }

    private void AudioReadFailed(Exception ex)
    {
        audioReadFailed = true;
        if (!reportedAudioFailure)
        {
            AudioDiagnostics.Log.Failure("refresh flyout audio", ex, defaultDevice?.Id);
            reportedAudioFailure = true;
        }
        recoveryTimer.Start();
    }

    private void UpdateMeters()
    {
        try
        {
            var peaks = monitor?.Peaks() ?? (0f, 0f);

            leftMeter.Value = peaks.Item1;
            rightMeter.Value = peaks.Item2;
        }
        catch (Exception ex) when (AudioUnavailableException.IsUnavailable(ex))
        {
            ClearMonitor();
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioReadFailed(ex);
            ClearMonitor();
        }
    }

    private void RefreshMonitors()
    {
        try
        {
            try
            {
                // In combined mode the tray follows playback, even when the user temporarily
                // opens only recording devices. That requires a separate volume subscription.
                trayDevice =
                    app.Settings.ShowBothDeviceGroups && direction == Direction.Recording
                        ? ReadDevices(Direction.Playback).FirstOrDefault(d => d.Multimedia)
                        : null;
                if (trayMonitor?.Id != trayDevice?.Id)
                {
                    var previousTray = trayMonitor;
                    trayMonitor = null;
                    previousTray?.Dispose();
                    if (trayDevice is not null)
                    {
                        trayMonitor = app.Audio.Monitor(trayDevice.Id);
                        var subscribedTray = trayMonitor;
                        // A callback may already be queued when an endpoint is replaced.
                        trayMonitor.Changed += state =>
                            Dispatcher.BeginInvoke(() =>
                            {
                                if (ReferenceEquals(trayMonitor, subscribedTray))
                                {
                                    trayState = state;
                                    app.UpdateTray(trayDevice, state);
                                }
                            });
                    }
                }
                trayState = trayMonitor?.State;
            }
            catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
            {
                AudioReadFailed(ex);
                var previousTray = trayMonitor;
                trayMonitor = null;
                trayState = null;
                previousTray?.Dispose();
            }

            if (monitor?.Id != defaultDevice?.Id)
            {
                var previous = monitor;
                monitor = null;
                previous?.Dispose();

                if (defaultDevice is not null)
                {
                    monitor = app.Audio.Monitor(defaultDevice.Id);
                    var subscribed = monitor;
                    // Ignore queued notifications from a monitor we have since disposed.
                    monitor.Changed += state =>
                        Dispatcher.BeginInvoke(() =>
                        {
                            if (ReferenceEquals(monitor, subscribed))
                            {
                                UpdateState(state);
                            }
                        });
                }
            }

            UpdateState(monitor?.State);
            if (
                monitor is null
                || audioReadFailed
                || !app.Audio.NotificationsAvailable
                || app.Settings.ShowBothDeviceGroups
                    && direction == Direction.Recording
                    && trayMonitor is null
            )
            {
                recoveryTimer.Start();
            }
            else
            {
                recoveryTimer.Stop();
                reportedAudioFailure = false;
            }
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioReadFailed(ex);
            ClearMonitor();
        }
    }

    private void UpdateState(AudioState? state)
    {
        // Updating the slider from Windows must not write the value back to the endpoint.
        updatingVolume = true;
        volumeSlider.IsEnabled = state is not null;
        if (state is null)
        {
            leftMeter.Value = rightMeter.Value = 0;
        }

        volumeSlider.Value = state?.Volume ?? 0;

        volumeSlider.ToolTip =
            state is null ? "No default audio device"
            : state.Muted ? "Muted"
            : $"Volume {state.Volume:P0}";

        volumeSlider.Opacity = state?.Muted == true ? .5 : 1;
        updatingVolume = false;

        if (app.Settings.ShowBothDeviceGroups && direction == Direction.Recording)
        {
            app.UpdateTray(trayDevice, trayState);
        }
        else
        {
            app.UpdateTray(defaultDevice, state);
        }
    }

    private void ClearMonitor()
    {
        var previousTray = trayMonitor;
        trayMonitor = null;
        trayState = null;
        previousTray?.Dispose();
        var previous = monitor;
        monitor = null;
        previous?.Dispose();
        volumeSlider.IsEnabled = false;
        leftMeter.Value = rightMeter.Value = 0;
        UpdateState(null);
        // Notifications can precede endpoint readiness, including with the same ID.
        recoveryTimer.Start();
    }
}
