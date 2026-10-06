using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using AudioSwitch.Shell;

namespace AudioSwitch.Settings;

internal sealed partial class SettingsWindow : Window
{
    private readonly App app;
    private readonly AppSettings draft;
    private readonly TabControl tabs = new();
    private readonly TextBlock statusText = new()
    {
        Foreground = Brushes.Firebrick,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(8),
        MaxWidth = 600,
    };
    private readonly TextBlock skinInfo = new()
    {
        Margin = new Thickness(0, 8, 0, 8),
        TextWrapping = TextWrapping.Wrap,
    };
    private readonly ObservableCollection<HotkeySettings> hotkeys;
    private readonly List<Action> commitEditors = [];
    private bool initialized;
    private TabItem osdTab = null!;
    private bool startAtLogin;
    private TextBox timeoutInput = null!;

    internal SettingsWindow(App app, AppSettings draft)
    {
        this.app = app;
        this.draft = draft;
        hotkeys = new(draft.Hotkeys);
        Title = "AudioSwitch Settings";
        Width = 700;
        Height = 620;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        UseLayoutRounding = true;
        SetResourceReference(BackgroundProperty, "ApplicationBackgroundBrush");
        SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
        statusText.SetResourceReference(
            TextBlock.ForegroundProperty,
            "SystemFillColorCriticalBrush"
        );
        var root = new DockPanel { Margin = new Thickness(12) };
        Content = root;
        var bottom = new DockPanel
        {
            Height = 54,
            Margin = new Thickness(0, 8, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        DockPanel.SetDock(bottom, Dock.Bottom);
        root.Children.Add(bottom);
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        DockPanel.SetDock(buttons, Dock.Right);
        bottom.Children.Add(buttons);
        buttons.Children.Add(Button("Apply", Save));
        buttons.Children.Add(
            Button(
                "Save & Close",
                () =>
                {
                    if (Save())
                    {
                        Close();
                    }
                }
            )
        );
        var cancel = Button("Cancel", Close);
        cancel.IsCancel = true;
        buttons.Children.Add(cancel);
        bottom.Children.Add(statusText);
        root.Children.Add(tabs);
        Tab("General", CreateGeneralTab());
        Tab("Playback", CreateDevicesTab(Direction.Playback));
        Tab("Recording", CreateDevicesTab(Direction.Recording));
        Tab("Hotkeys", CreateHotkeysTab());
        osdTab = Tab("OSD", CreateOsdTab());
        Tab("About", CreateAboutTab());
        tabs.SelectionChanged += (_, e) =>
        {
            if (!ReferenceEquals(e.Source, tabs))
            {
                return;
            }

            if (tabs.SelectedItem == osdTab)
            {
                Preview();
            }
            else
            {
                app.Osd.EndPreview();
            }
        };
        app.Osd.Moved += OsdMoved;
        Closed += (_, _) =>
        {
            app.Osd.Moved -= OsdMoved;
            app.Osd.EndPreview();
        };
        initialized = true;
    }

    private bool Save()
    {
        try
        {
            if (!int.TryParse(timeoutInput.Text, out var timeout) || timeout is < 100 or > 100000)
            {
                throw new InvalidOperationException(
                    "OSD duration must be between 100 and 100000 milliseconds."
                );
            }

            foreach (var commit in commitEditors)
            {
                commit();
            }

            draft.Hotkeys = hotkeys.ToList();
            if (draft.VolumeScroll && draft.ScrollKeys == 0)
            {
                throw new InvalidOperationException(
                    "Select a modifier for global volume scrolling."
                );
            }

            app.SaveSettings(draft.Clone());
            if (app.IsDefaultProfile)
            {
                StartupRegistration.SetEnabled(startAtLogin);
            }

            statusText.Text = "Saved.";
            return true;
        }
        catch (Exception ex)
        {
            statusText.Text = ex.Message;
            return false;
        }
    }
}
