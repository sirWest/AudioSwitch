using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed partial class UiTests
{
    [UiFact]
    public void HotkeyEditorShowsRelevantOptionsPreservesUnavailableDevicesAndScrolls()
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
            "AudioSwitch",
            "bin",
            configuration,
            "net10.0-windows",
            "AudioSwitch.exe"
        );
        var temp = Path.Combine(
            Path.GetTempPath(),
            "AudioSwitch.UiTests",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(temp);
        var config = Path.Combine(temp, "settings.json");
        var seed = new AppSettings
        {
            Hotkeys = Enumerable
                .Range(0, 16)
                .Select(i => new HotkeySettings
                {
                    Name = $"Shortcut {i}",
                    VirtualKey = 112 + i,
                    Modifiers = 6,
                })
                .ToList(),
        };
        seed.Hotkeys[0].Function = HotkeyAction.SelectAudioDevices;
        seed.Hotkeys[0].Playback = new()
        {
            DeviceId = "unavailable-test-output",
            DeviceName = "Test headphones",
        };
        seed.Hotkeys[0].Recording = new() { MuteOthers = true };
        new SettingsStore(config).Save(seed);
        var start = new ProcessStartInfo(exe) { UseShellExecute = false };
        foreach (var argument in new[] { "--settings", "--exit-with-settings", "--config", config })
        {
            start.ArgumentList.Add(argument);
        }
        using var process = Process.Start(start)!;
        try
        {
            AutomationElement? settings = null;
            Wait(
                () => (settings = Window(process.Id, "AudioSwitch Settings")) is not null,
                "Settings did not appear."
            );
            Select(Find(settings!, ControlType.TabItem, "Hotkeys"));
            var grid = Find(settings!, ControlType.DataGrid);
            var scroll = (ScrollPattern)grid.GetCurrentPattern(ScrollPattern.Pattern);
            Assert.True(scroll.Current.VerticallyScrollable);
            var row = Find(grid, ControlType.DataItem);
            Select(row);
            Invoke(Find(settings!, ControlType.Button, "Edit"));
            AutomationElement? editor = null;
            AutomationElement? Editor(string title) =>
                settings!.FindFirst(
                    TreeScope.Descendants,
                    new AndCondition(
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.Window
                        ),
                        new PropertyCondition(AutomationElement.NameProperty, title)
                    )
                ) ?? Window(process.Id, title);
            Wait(
                () => (editor = Editor("Edit hotkey")) is not null,
                "Hotkey editor did not appear."
            );
            var playback = Find(editor!, ControlType.ComboBox, "Playback device");
            Assert.Contains(
                "Test headphones (unavailable)",
                ((SelectionPattern)playback.GetCurrentPattern(SelectionPattern.Pattern))
                    .Current.GetSelection()
                    .Single()
                    .Current.Name
            );
            var muteAll = Find(editor!, ControlType.CheckBox, "Mute all recording devices");
            Assert.Equal(
                ToggleState.On,
                ((TogglePattern)muteAll.GetCurrentPattern(TogglePattern.Pattern))
                    .Current
                    .ToggleState
            );
            var action = Find(editor!, ControlType.ComboBox, "Action");
            Assert.True(
                playback.Current.BoundingRectangle.Left > action.Current.BoundingRectangle.Right
            );
            var pairWidth = editor!.Current.BoundingRectangle.Width;
            Choose(action, "Playback volume up");
            Assert.True(playback.Current.IsOffscreen);
            Wait(
                () => editor.Current.BoundingRectangle.Width < pairWidth,
                "Simple editor did not shrink."
            );
            Choose(action, "Select audio devices");
            playback = Find(editor!, ControlType.ComboBox, "Playback device");
            Assert.False(playback.Current.IsOffscreen);
            Wait(
                () => editor.Current.BoundingRectangle.Width >= pairWidth,
                "Device editor did not expand."
            );
            Assert.True(
                playback.Current.BoundingRectangle.Left > action.Current.BoundingRectangle.Right
            );
            var monitor = new MonitorInfo
            {
                Size = System.Runtime.InteropServices.Marshal.SizeOf<MonitorInfo>(),
            };
            Assert.True(
                GetMonitorInfo(MonitorFromWindow(editor.Current.NativeWindowHandle, 2), ref monitor)
            );
            var editorBounds = editor.Current.BoundingRectangle;
            Assert.True(
                editorBounds.Left >= monitor.Work.Left && editorBounds.Right <= monitor.Work.Right
            );
            Assert.True(
                editorBounds.Top >= monitor.Work.Top && editorBounds.Bottom <= monitor.Work.Bottom
            );
            Choose(Find(editor!, ControlType.ComboBox, "On selection"), "Unmute");
            Invoke(Find(editor!, ControlType.Button, "Save"));
            Wait(() => Editor("Edit hotkey") is null, "Editor did not close.");
            Invoke(Find(settings!, ControlType.Button, "Duplicate"));
            Wait(
                () => (editor = Editor("Add hotkey")) is not null,
                "Duplicate editor did not appear."
            );
            Invoke(Find(editor!, ControlType.Button, "Save"));
            Find(editor!, ControlType.Text, "Choose a shortcut.");
            Invoke(Find(editor!, ControlType.Button, "Cancel"));
            Invoke(Find(settings!, ControlType.Button, "Save & Close"));
            Assert.True(process.WaitForExit(10000));
            var saved = new SettingsStore(config).Load();
            Assert.Equal(16, saved.Hotkeys.Count);
            Assert.Equal("unavailable-test-output", saved.Hotkeys[0].Playback.DeviceId);
            Assert.Equal(HotkeyMuteAction.Unmute, saved.Hotkeys[0].Playback.MuteAction);
            Assert.True(saved.Hotkeys[0].Recording.MuteOthers);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
            Directory.Delete(temp, recursive: true);
        }
    }
}
