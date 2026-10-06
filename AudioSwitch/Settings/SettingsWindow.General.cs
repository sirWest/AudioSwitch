using System.Windows;
using System.Windows.Controls;
using AudioSwitch.Core.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow
{
    private UIElement CreateGeneralTab()
    {
        var panel = new StackPanel();
        var selectors = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        selectors.ColumnDefinitions.Add(new());
        selectors.ColumnDefinitions.Add(new());
        panel.Children.Add(selectors);
        var appearance = new StackPanel();
        var group = new StackPanel();
        Grid.SetColumn(group, 1);
        selectors.Children.Add(appearance);
        selectors.Children.Add(group);
        var theme = Choices(
            appearance,
            "Appearance",
            draft,
            nameof(draft.Theme),
            Enum.GetValues<AppTheme>()
        );
        theme.SelectionChanged += (_, _) =>
        {
            if (theme.SelectedItem is AppTheme selected)
            {
                app.ApplyTheme(selected);
            }
        };
        Choices(
            group,
            "Default device group",
            draft,
            nameof(draft.DefaultDeviceGroup),
            Enum.GetValues<DeviceGroup>()
        );
        panel.Children.Add(
            Check("Also change the communications default", draft, nameof(draft.AlsoCommunications))
        );
        panel.Children.Add(Check("Show hardware names", draft, nameof(draft.ShowHardwareName)));
        panel.Children.Add(Check("Colour VU meters", draft, nameof(draft.ColorVu)));
        panel.Children.Add(
            Check("Close the device list after selecting", draft, nameof(draft.CloseAfterSelecting))
        );
        panel.Children.Add(
            Check("Keep the device window visible", draft, nameof(draft.AlwaysVisible))
        );
        panel.Children.Add(
            Check(
                "Right-click tray icon to switch to the next device",
                draft,
                nameof(draft.QuickSwitch)
            )
        );
        panel.Children.Add(
            Check("Show OSD when quick-switching", draft, nameof(draft.QuickSwitchOsd))
        );
        startAtLogin = StartupRegistration.IsEnabled;

        var startup = new CheckBox
        {
            Content = "Start AudioSwitch at sign-in",
            MinHeight = 24,
            Padding = new Thickness(8, 0, 0, 0),
            IsChecked = startAtLogin,
            IsEnabled = app.IsDefaultProfile,
            Margin = new Thickness(0, 2, 0, 2),
        };
        startup.Checked += (_, _) => startAtLogin = true;
        startup.Unchecked += (_, _) => startAtLogin = false;
        panel.Children.Add(startup);
        Label(panel, "Mouse-wheel volume control");
        panel.Children.Add(
            Check("Enable global volume scrolling", draft, nameof(draft.VolumeScroll))
        );
        panel.Children.Add(new ScrollModifierPicker(draft));
        panel.Children.Add(Check("Show OSD when scrolling", draft, nameof(draft.ScrollOsd)));
        return panel;
    }
}
