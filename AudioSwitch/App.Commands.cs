using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Imaging;
using AudioSwitch.Shell;

namespace AudioSwitch;

public sealed partial class App
{
    private void SubscribeTrayEvents()
    {
        tray.DisplayChanged += () =>
            Dispatcher.BeginInvoke(new Action(() => RunSafely(flyout.Refresh)));
        tray.LeftClick += ToggleFlyout;
        tray.RightClick += () =>
        {
            if (
                Settings.QuickSwitch
                && !flyout.IsVisible
                && DateTime.UtcNow - closedFromTray >= TimeSpan.FromMilliseconds(400)
                && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
            )
            {
                RunSafely(() => Cycle(Settings.DefaultDirection, false, Settings.QuickSwitchOsd));
            }
            else
            {
                ShowMenu();
            }
        };
        shortcuts.Pressed += hotkey => RunSafely(() => ExecuteHotkey(hotkey));
        shortcuts.Scrolled += delta =>
            RunSafely(() =>
                ChangeVolume(Settings.DefaultDirection, delta, Settings.ScrollOsd, true)
            );
    }

    private void ToggleFlyout()
    {
        if (DateTime.UtcNow - closedFromTray < TimeSpan.FromMilliseconds(400))
        {
            closedFromTray = default;
            return;
        }

        if (flyout.IsVisible)
        {
            flyout.Dismiss();
            return;
        }

        var direction = Settings.DefaultDirection;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            direction = direction == Direction.Playback ? Direction.Recording : Direction.Playback;
        }

        // Match ShowMenu's foreground handoff before creating/showing the flyout.
        Native.SetForegroundWindow(tray.Handle);
        RunSafely(() =>
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                flyout.Open(direction);
            }
            else
            {
                flyout.Open();
            }
        });
    }

    internal void PositionFlyout(Window window) => tray.Position(window);

    internal Native.FlyoutAnimation PrepareFlyoutAnimation(Window window) =>
        new(window, tray.Anchor());

    internal Native.FlyoutAnimation PrepareFlyoutDismissal(Window window, Action completed) =>
        new(window, tray.Anchor(), true, completed);

    internal void FlyoutDeactivated()
    {
        Native.GetCursorPos(out var point);
        var anchor = tray.Anchor();
        if (
            point.X >= anchor.Left
            && point.X < anchor.Right
            && point.Y >= anchor.Top
            && point.Y < anchor.Bottom
        )
        {
            closedFromTray = DateTime.UtcNow;
        }
    }

    internal void Select(AudioDevice device)
    {
        Audio.Select(device, Settings);
        audioRefresh?.Request();
        if (Settings.CloseAfterSelecting && !Settings.AlwaysVisible)
        {
            flyout.Dismiss();
        }
    }

    private void Cycle(Direction direction, bool previous, bool show)
    {
        var device = Audio.Cycle(direction, previous, Settings);
        if (show)
        {
            ShowFeedback(device.DisplayName(Settings), null);
        }

        flyout.Refresh();
    }

    private void ExecuteHotkey(HotkeySettings hotkey)
    {
        switch (hotkey.Function)
        {
            case HotkeyAction.SelectAudioDevices:
                var executor = new DeviceHotkeyExecutor(Audio);
                var completed = executor.Execute(hotkey, Settings);
                audioRefresh?.Request();
                if (executor.Failures.Count > 0)
                    throw new AggregateException(
                        "Some device hotkey operations failed. Windows' actual device state is shown.",
                        executor.Failures
                    );
                if (hotkey.ShowOsd && completed.Count > 0)
                {
                    ShowFeedback(string.Join("; ", completed), null);
                }
                break;
            case HotkeyAction.PreviousPlaybackDevice:
                Cycle(Direction.Playback, previous: true, hotkey.ShowOsd);
                break;
            case HotkeyAction.NextPlaybackDevice:
                Cycle(Direction.Playback, previous: false, hotkey.ShowOsd);
                break;
            case HotkeyAction.PreviousRecordingDevice:
                Cycle(Direction.Recording, previous: true, hotkey.ShowOsd);
                break;
            case HotkeyAction.NextRecordingDevice:
                Cycle(Direction.Recording, previous: false, hotkey.ShowOsd);
                break;
            case HotkeyAction.TogglePlaybackMute:
                ToggleMute(Direction.Playback, hotkey.ShowOsd);
                break;
            case HotkeyAction.ToggleRecordingMute:
                ToggleMute(Direction.Recording, hotkey.ShowOsd);
                break;
            case HotkeyAction.PlaybackVolumeUp:
                ChangeVolume(Direction.Playback, 1, hotkey.ShowOsd);
                break;
            case HotkeyAction.PlaybackVolumeDown:
                ChangeVolume(Direction.Playback, -1, hotkey.ShowOsd);
                break;
            case HotkeyAction.RecordingVolumeUp:
                ChangeVolume(Direction.Recording, 1, hotkey.ShowOsd);
                break;
            case HotkeyAction.RecordingVolumeDown:
                ChangeVolume(Direction.Recording, -1, hotkey.ShowOsd);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(hotkey),
                    hotkey.Function,
                    "Unknown hotkey action."
                );
        }
    }

    private void ToggleMute(Direction direction, bool showOsd)
    {
        var id = RequireDefault(direction);
        if (Settings.ForDevice(id)?.ExcludeFromHotkeyMute == true)
        {
            return;
        }
        var state = Audio.SetMute(id);
        if (showOsd)
        {
            ShowVolumeFeedback(direction, state);
        }
    }

    private string RequireDefault(Direction direction) =>
        Audio.DefaultId(direction)
        ?? throw new AudioUnavailableException("No default audio device is available.");

    internal void ChangeVolume(Direction direction, int delta, bool show, bool accelerate = false)
    {
        var id = RequireDefault(direction);
        var state = Audio.State(id);
        var step =
            accelerate && DateTime.UtcNow - lastScroll < TimeSpan.FromMilliseconds(80)
                ? .04f
                : .02f;
        lastScroll = DateTime.UtcNow;
        state = Audio.SetVolume(id, state.Volume + Math.Sign(delta) * step);
        if (show)
        {
            ShowVolumeFeedback(direction, state);
        }
    }

    private void ShowFeedback(string? device, AudioState? state)
    {
        if (Osd.Preview)
        {
            return;
        }

        if (Settings.CustomOsd)
        {
            Osd.Display(Settings.Osd, state?.Volume ?? .75f, state?.Muted ?? false, device);
        }
        else if (device is not null)
        {
            tray.Notify(device);
        }
    }

    private void ShowVolumeFeedback(Direction direction, AudioState state)
    {
        if (Osd.Preview)
        {
            return;
        }

        if (Settings.CustomOsd)
        {
            ShowFeedback(null, state);
        }
        else if (direction == Direction.Playback)
        {
            Native.TryShowVolumeOsd();
        }
    }

    internal void UpdateTray(AudioDevice? device, AudioState? state)
    {
        if (device is null || state is null)
        {
            var unavailableKey = $"unavailable:{tray.IconSize}";
            var unavailableIcon =
                iconKey != unavailableKey
                    ? Images.Icon(Images.TrayAsset("mute.png"), tray.IconSize)
                    : 0;
            tray.Update("AudioSwitch - No default audio device", unavailableIcon);
            iconKey = unavailableKey;
            return;
        }

        var pref = Settings.ForDevice(device.Id);
        var image = Images.VolumeAsset(state.Volume, state.Muted);
        var key = $"{image}:{pref?.Hue}:{pref?.Saturation}:{pref?.Brightness}:{tray.IconSize}";
        var icon =
            key != iconKey
                ? Images.Icon(Images.Tint(Images.TrayAsset(image), pref), tray.IconSize)
                : 0;
        tray.Update(device.DisplayName(Settings), icon);
        iconKey = key;
    }

    private void ShowMenu()
    {
        var menu = new ContextMenu();
        void Item(string name, Action action)
        {
            var item = new MenuItem { Header = name };
            item.Click += (_, _) => RunSafely(action);
            menu.Items.Add(item);
        }

        Item("Playback devices", () => flyout.Open(Direction.Playback));
        Item("Recording devices", () => flyout.Open(Direction.Recording));
        menu.Items.Add(new Separator());
        Item("Settings", OpenSettings);
        Item(
            "Windows sound settings",
            () =>
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo("ms-settings:sound")
                    {
                        UseShellExecute = true,
                    }
                )
        );
        Item(
            "Classic sound control panel",
            () =>
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo("control.exe", "mmsys.cpl")
                    {
                        UseShellExecute = true,
                    }
                )
        );
        menu.Items.Add(new Separator());
        Item("Exit", Shutdown);
        tray.ShowMenu(menu);
    }
}
