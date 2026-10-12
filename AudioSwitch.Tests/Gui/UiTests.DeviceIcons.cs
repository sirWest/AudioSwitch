using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed partial class UiTests
{
    [UiFact]
    public void DeviceIconsCanBeImportedResetAndSelectedForBothDirections()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AudioSwitch.sln")))
            root = root.Parent;
        Assert.NotNull(root);
        var exe = Path.Combine(AppContext.BaseDirectory, "AudioSwitch.exe");
        if (!File.Exists(exe))
            exe = Path.Combine(
                root.FullName,
                "AudioSwitch",
                "bin",
                new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name,
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
        var iconDirectory = Path.Combine(temp, "DeviceIcons");
        var source = Path.Combine(root.FullName, "AudioSwitch", "Assets", "75-100.png");
        var start = new ProcessStartInfo(exe) { UseShellExecute = false };
        foreach (var argument in new[] { "--settings", "--exit-with-settings", "--config", config })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        try
        {
            AutomationElement? window = null;
            Wait(
                () => (window = Window(process.Id, "AudioSwitch Settings")) is not null,
                "Settings did not open."
            );
            var imported = 0;
            var lastTab = "Playback";
            foreach (var direction in new[] { "Playback", "Recording" })
            {
                Select(Find(window!, ControlType.TabItem, direction));
                var rows = Find(window!, ControlType.List)
                    .FindAll(
                        TreeScope.Children,
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.ListItem
                        )
                    );
                if (rows.Count == 0)
                    continue;
                lastTab = direction;
                Select(rows[0]);
                Assert.NotNull(
                    Find(
                        window!,
                        ControlType.CheckBox,
                        $"Default {direction.ToLowerInvariant()} device at sign-in"
                    )
                );
                Assert.NotNull(
                    Find(
                        window!,
                        ControlType.CheckBox,
                        $"Default communications {direction.ToLowerInvariant()} at sign-in"
                    )
                );
                var tray = Find(window!, ControlType.CheckBox, "Use device icon in tray");
                var toggle = (TogglePattern)tray.GetCurrentPattern(TogglePattern.Pattern);
                AutomationElement[] Sliders() =>
                    window!
                        .FindAll(
                            TreeScope.Descendants,
                            new PropertyCondition(
                                AutomationElement.ControlTypeProperty,
                                ControlType.Slider
                            )
                        )
                        .Cast<AutomationElement>()
                        .ToArray();
                Assert.All(Sliders(), slider => Assert.True(slider.Current.IsEnabled));
                Assert.All(
                    Sliders(),
                    slider =>
                    {
                        Assert.False(slider.Current.IsOffscreen);
                        Assert.True(
                            window!.Current.BoundingRectangle.Contains(
                                slider.Current.BoundingRectangle
                            )
                        );
                    }
                );
                toggle.Toggle();
                Wait(
                    () => Sliders().All(s => !s.Current.IsEnabled),
                    "System icon mode did not disable colors."
                );
                var previews = window!
                    .FindAll(
                        TreeScope.Descendants,
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.Image
                        )
                    )
                    .Cast<AutomationElement>()
                    .Where(i => i.Current.Name.StartsWith("Tray icon "))
                    .ToArray();
                Assert.Single(previews);
                Assert.True(previews[0].Current.IsEnabled);
                toggle.Toggle();
                Wait(
                    () => Sliders().All(s => s.Current.IsEnabled),
                    "Volume mode did not restore colors."
                );
                if (direction == "Recording")
                    toggle.Toggle();
                var trayStateBeforeImport = toggle.Current.ToggleState;
                Invoke(Find(window!, ControlType.Button, "Browse..."));
                AutomationElement? Picker() =>
                    AutomationElement.RootElement.FindFirst(
                        TreeScope.Descendants,
                        new AndCondition(
                            new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id),
                            new PropertyCondition(
                                AutomationElement.NameProperty,
                                "Choose device icon"
                            )
                        )
                    );
                AutomationElement? picker = null;
                Wait(() => (picker = Picker()) is not null, "Icon picker did not open.");
                var filename = Find(picker!, ControlType.Edit, "File name:");
                filename.SetFocus();
                ((ValuePattern)filename.GetCurrentPattern(ValuePattern.Pattern)).SetValue(source);
                Assert.Equal(
                    source,
                    ((ValuePattern)filename.GetCurrentPattern(ValuePattern.Pattern)).Current.Value
                );
                var open = picker!.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "1")
                );
                Assert.NotNull(open);
                // The shell exposes Open as a pane; its Invoke provider can navigate instead.
                Assert.True(
                    PostMessage(
                        (nint)picker.Current.NativeWindowHandle,
                        0x111,
                        1,
                        (nint)open.Current.NativeWindowHandle
                    )
                );
                Wait(() => Picker() is null, "Icon picker did not close.");
                Wait(
                    () => Find(window!, ControlType.Button, "Reset icon").Current.IsEnabled,
                    "Custom icon was not imported."
                );
                Assert.True(tray.Current.IsEnabled);
                Assert.Equal(trayStateBeforeImport, toggle.Current.ToggleState);
                Assert.All(
                    Sliders(),
                    slider =>
                        Assert.Equal(
                            trayStateBeforeImport == ToggleState.Off,
                            slider.Current.IsEnabled
                        )
                );
                if (trayStateBeforeImport == ToggleState.Off)
                    toggle.Toggle();
                if (imported == 0)
                    Assert.False(Directory.Exists(iconDirectory));

                var capture = new ProcessStartInfo("powershell.exe")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                foreach (
                    var argument in new[]
                    {
                        "-NoProfile",
                        "-ExecutionPolicy",
                        "Bypass",
                        "-File",
                        Path.Combine(root.FullName, "tools", "Capture-Window.ps1"),
                        "-ProcessId",
                        process.Id.ToString(),
                        "-Output",
                        Path.Combine(
                            root.FullName,
                            "artifacts",
                            "device-icon-" + direction.ToLowerInvariant() + ".png"
                        ),
                    }
                )
                    capture.ArgumentList.Add(argument);
                using (var screenshot = Process.Start(capture)!)
                {
                    Assert.True(screenshot.WaitForExit(10000));
                    Assert.Equal(0, screenshot.ExitCode);
                }
                Invoke(Find(window!, ControlType.Button, "Apply"));
                imported++;
                Wait(
                    () =>
                        File.Exists(config)
                        && new SettingsStore(config)
                            .Load()
                            .Devices.Count(d => d.CustomIcon.Length > 0) == imported,
                    "Custom icon was not saved."
                );
                Assert.Single(Directory.GetFiles(iconDirectory, "*.png"));
            }
            Assert.True(imported > 0, "At least one audio endpoint is needed for this UI test.");
            Select(Find(window!, ControlType.TabItem, lastTab));
            Invoke(Find(window!, ControlType.Button, "Reset icon"));
            Invoke(Find(window!, ControlType.Button, "Apply"));
            Wait(
                () =>
                    new SettingsStore(config).Load().Devices.Count(d => d.CustomIcon.Length > 0)
                    == imported - 1,
                "Reset icon was not saved."
            );
            var beforeCancel = File.ReadAllText(config);
            var restoredTray = Find(window!, ControlType.CheckBox, "Use device icon in tray");
            Assert.True(restoredTray.Current.IsEnabled);
            ((TogglePattern)restoredTray.GetCurrentPattern(TogglePattern.Pattern)).Toggle();
            var enabledSliders = window!
                .FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Slider)
                )
                .Cast<AutomationElement>();
            Assert.All(enabledSliders, slider => Assert.True(slider.Current.IsEnabled));
            Invoke(Find(window!, ControlType.Button, "Reset device"));
            Invoke(Find(window!, ControlType.Button, "Cancel"));
            Assert.True(process.WaitForExit(5000));
            Assert.Equal(beforeCancel, File.ReadAllText(config));

            using var reopened = Process.Start(start)!;
            try
            {
                Wait(
                    () => (window = Window(reopened.Id, "AudioSwitch Settings")) is not null,
                    "Settings did not reopen."
                );
                Select(Find(window!, ControlType.TabItem, lastTab));
                var saved = new SettingsStore(config).Load();
                var device = Assert.Single(saved.Devices, d => d.Direction.ToString() == lastTab);
                Assert.True(device.UseDeviceIconInTray);
                Invoke(Find(window!, ControlType.Button, "Reset device"));
                Assert.Equal(beforeCancel, File.ReadAllText(config));
                Invoke(Find(window!, ControlType.Button, "Apply"));
                Wait(
                    () => new SettingsStore(config).Load().ForDevice(device.Id) is null,
                    "Apply did not remove the reset device preferences."
                );
                var customName = Find(window!, ControlType.CheckBox, "Use custom name");
                ((TogglePattern)customName.GetCurrentPattern(TogglePattern.Pattern)).Toggle();
                (
                    (ValuePattern)
                        Find(window!, ControlType.Edit).GetCurrentPattern(ValuePattern.Pattern)
                ).SetValue("Saved with main buttons");
                Invoke(Find(window!, ControlType.Button, "Save & Close"));
                Assert.True(reopened.WaitForExit(5000));
                Assert.Equal(
                    "Saved with main buttons",
                    new SettingsStore(config).Load().ForDevice(device.Id)?.CustomName
                );
            }
            finally
            {
                if (!reopened.HasExited)
                {
                    reopened.Kill();
                    reopened.WaitForExit(5000);
                }
            }
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
                process.WaitForExit(5000);
            }
            Directory.Delete(temp, true);
        }
    }
}
