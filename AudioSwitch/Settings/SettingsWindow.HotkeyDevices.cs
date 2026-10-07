using System.Windows;
using System.Windows.Controls;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private void AddHotkeyDeviceEditor(
        Panel parent,
        string title,
        Direction direction,
        HotkeyDeviceSettings options
    )
    {
        var section = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        parent.Children.Add(section);
        IReadOnlyList<AudioDevice> available;
        try
        {
            available = app.Audio.List(direction, draft);
        }
        catch (Exception ex) when (AudioOperationException.IsDeviceFailure(ex))
        {
            AudioDiagnostics.Log.Failure($"list {direction} hotkey choices", ex);
            available = [];
        }
        var choices = new List<HotkeyOption<string>> { new("", "Leave unchanged") };
        choices.AddRange(
            available.Select(device => new HotkeyOption<string>(
                device.Id,
                device.DisplayName(draft)
            ))
        );
        if (options.DeviceId is not null && available.All(d => d.Id != options.DeviceId))
        {
            choices.Add(
                new(
                    options.DeviceId,
                    HotkeyPresentation.DeviceName(options, draft) + " (unavailable)"
                )
            );
        }
        var deviceChoice = HotkeyChoice(
            section,
            title + " device",
            choices.Select(c => c.Value),
            options.DeviceId ?? "",
            id => choices.First(c => c.Value == id).Label
        );
        var mutePanel = new StackPanel();
        section.Children.Add(mutePanel);
        var muteChoice = HotkeyChoice(
            mutePanel,
            "On selection",
            Enum.GetValues<HotkeyMuteAction>(),
            options.MuteAction,
            HotkeyPresentation.MuteName
        );
        muteChoice.SelectionChanged += (_, _) =>
            options.MuteAction = (HotkeyMuteAction)muteChoice.SelectedValue;
        var excluded = new TextBlock
        {
            Text = "Excluded from hotkey mute/unmute",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 4),
        };
        section.Children.Add(excluded);
        var others = Check("", options, nameof(options.MuteOthers));
        section.Children.Add(others);
        void UpdateDevice()
        {
            var id = (string)deviceChoice.SelectedValue;
            options.DeviceId = id.Length == 0 ? null : id;
            var selected = available.FirstOrDefault(d => d.Id == id);
            if (selected is not null)
            {
                options.DeviceName = selected.DisplayName(draft);
            }
            var isExcluded = draft.ForDevice(id)?.ExcludeFromHotkeyMute == true;
            mutePanel.Visibility =
                options.DeviceId is not null && !isExcluded
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            excluded.Visibility =
                options.DeviceId is not null && isExcluded
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            others.Content = options.DeviceId is null
                ? $"Mute all {title.ToLowerInvariant()} devices"
                : $"Mute other {title.ToLowerInvariant()} devices";
            others.ToolTip = "Devices excluded from hotkey mute/unmute are left unchanged.";
        }
        deviceChoice.SelectionChanged += (_, _) => UpdateDevice();
        UpdateDevice();
    }
}
