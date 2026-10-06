using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using AudioSwitch.Core.Audio;
using AudioSwitch.Core.Settings;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class UiTests
{
    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindowEx(
        nint parent,
        nint after,
        string? className,
        string title
    );

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint process);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(nint window, uint message, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        nint window,
        nint after,
        int x,
        int y,
        int width,
        int height,
        uint flags
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct ScreenBounds
    {
        public int Left,
            Top,
            Right,
            Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public ScreenBounds Monitor,
            Work;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint window, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    private static AutomationElement? Window(int process, string title) =>
        AutomationElement.RootElement.FindFirst(
            TreeScope.Children,
            new AndCondition(
                new PropertyCondition(AutomationElement.ProcessIdProperty, process),
                new PropertyCondition(AutomationElement.NameProperty, title)
            )
        );

    private static AutomationElement Find(
        AutomationElement parent,
        ControlType type,
        string? name = null
    )
    {
        Condition condition = new PropertyCondition(AutomationElement.ControlTypeProperty, type);
        if (name is not null)
        {
            condition = new AndCondition(
                condition,
                new PropertyCondition(AutomationElement.NameProperty, name)
            );
        }

        return parent.FindFirst(TreeScope.Descendants, condition)
            ?? throw new InvalidOperationException($"Missing {type.ProgrammaticName}: {name}");
    }

    private static void Select(AutomationElement element) =>
        ((SelectionItemPattern)element.GetCurrentPattern(SelectionItemPattern.Pattern)).Select();

    private static void Invoke(AutomationElement element) =>
        ((InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern)).Invoke();

    private static void Choose(AutomationElement combo, string name)
    {
        ((ExpandCollapsePattern)combo.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Expand();
        Select(Find(combo, ControlType.ListItem, name));
        ((ExpandCollapsePattern)combo.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Collapse();
    }

    private static void Wait(Func<bool> condition, string message)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            if (condition())
            {
                return;
            }

            Thread.Sleep(100);
        }

        throw new TimeoutException(message);
    }

    [UiFact]
    public void FlyoutReceivesFocusWithoutSelectingADeviceOnOpenAndReopen()
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
        new SettingsStore(Path.Combine(temp, "settings.json")).Save(
            new AppSettings { DefaultDeviceGroup = DeviceGroup.Both }
        );
        var start = new ProcessStartInfo(exe) { UseShellExecute = false };
        start.ArgumentList.Add("--show");
        start.ArgumentList.Add("--config");
        start.ArgumentList.Add(Path.Combine(temp, "settings.json"));
        using var process = Process.Start(start)!;
        try
        {
            AutomationElement? flyout = null;
            Wait(
                () => (flyout = Window(process.Id, "AudioSwitch")) is not null,
                "Flyout did not appear."
            );
            void AssertFocus()
            {
                Wait(
                    () => GetForegroundWindow() == flyout!.Current.NativeWindowHandle,
                    "Flyout did not become the foreground window without device selection."
                );
                Wait(
                    () => AutomationElement.FocusedElement?.Current.ProcessId == process.Id,
                    "Flyout did not receive keyboard focus without device selection."
                );
            }

            AssertFocus();
            void AssertBothGroups()
            {
                var playbackHeading = Find(flyout!, ControlType.Text, "Playback");
                var recordingHeading = Find(flyout!, ControlType.Text, "Recording");
                Assert.True(
                    playbackHeading.Current.BoundingRectangle.Top
                        < recordingHeading.Current.BoundingRectangle.Top
                );
            }
            AssertBothGroups();
            nint tray = 0;
            while ((tray = FindWindowEx(0, tray, null, "AudioSwitch messages")) != 0)
            {
                GetWindowThreadProcessId(tray, out var owner);
                if (owner == process.Id)
                {
                    break;
                }
            }

            Assert.NotEqual(0, tray);
            for (var attempt = 0; attempt < 2; attempt++)
            {
                // Exercise the tray callback without switching the default audio device.
                Assert.True(PostMessage(tray, 0x8001, 0, 0x400));
                Wait(
                    () => Window(process.Id, "AudioSwitch") is null,
                    "Tray toggle did not hide the flyout."
                );
                Assert.True(PostMessage(tray, 0x8001, 0, 0x400));
                Wait(
                    () => (flyout = Window(process.Id, "AudioSwitch")) is not null,
                    "Tray toggle did not reopen the flyout."
                );
                AssertFocus();
                AssertBothGroups();
            }

            Assert.True(PostMessage(tray, 0x8001, 0, 0x400));
            Wait(
                () => Window(process.Id, "AudioSwitch") is null,
                "Flyout did not hide before opening the tray menu."
            );
            Assert.True(PostMessage(tray, 0x8001, 0, 0x7b));
            AutomationElement? playback = null;
            Wait(
                () =>
                    (
                        playback = AutomationElement.RootElement.FindFirst(
                            TreeScope.Descendants,
                            new AndCondition(
                                new PropertyCondition(
                                    AutomationElement.ProcessIdProperty,
                                    process.Id
                                ),
                                new PropertyCondition(
                                    AutomationElement.NameProperty,
                                    "Playback devices"
                                )
                            )
                        )
                    )
                        is not null,
                "Tray menu did not appear."
            );
            Invoke(playback!);
            Wait(
                () => (flyout = Window(process.Id, "AudioSwitch")) is not null,
                "Tray menu did not open the flyout."
            );
            AssertFocus();
            Assert.NotNull(Find(flyout!, ControlType.Text, "Playback"));
            Assert.Null(
                flyout!.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "Recording")
                )
            );
            Assert.True(PostMessage(tray, 0x8001, 0, 0x400));
            Wait(() => Window(process.Id, "AudioSwitch") is null, "Playback flyout did not hide.");
            Assert.True(PostMessage(tray, 0x8001, 0, 0x7b));
            AutomationElement? recording = null;
            Wait(
                () =>
                    (
                        recording = AutomationElement.RootElement.FindFirst(
                            TreeScope.Descendants,
                            new AndCondition(
                                new PropertyCondition(
                                    AutomationElement.ProcessIdProperty,
                                    process.Id
                                ),
                                new PropertyCondition(
                                    AutomationElement.NameProperty,
                                    "Recording devices"
                                )
                            )
                        )
                    )
                        is not null,
                "Tray menu did not reappear."
            );
            Invoke(recording!);
            Wait(
                () => (flyout = Window(process.Id, "AudioSwitch")) is not null,
                "Recording flyout did not appear."
            );
            Assert.NotNull(Find(flyout!, ControlType.Text, "Recording"));
            Assert.Null(
                flyout!.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "Playback")
                )
            );
            Assert.True(PostMessage(tray, 0x8001, 0, 0x400));
            Wait(() => Window(process.Id, "AudioSwitch") is null, "Recording flyout did not hide.");
            Assert.True(PostMessage(tray, 0x8001, 0, 0x400));
            Wait(
                () => (flyout = Window(process.Id, "AudioSwitch")) is not null,
                "Combined flyout did not reopen."
            );
            AssertBothGroups();

            // Rebuilding either group must retain its heading and an empty-state row,
            // without selecting a device as a side effect of clearing the list.
            var devices = ReadAudioDevices();
            var defaults = devices
                .Where(device => device.Multimedia)
                .ToDictionary(device => device.Direction, device => device.Id);
            var profileStore = new SettingsStore(Path.Combine(temp, "settings.json"));
            var profile = profileStore.Load();
            profile.Devices = devices
                .Select(device => new DeviceSettings
                {
                    Id = device.Id,
                    Direction = device.Direction,
                    Hidden = true,
                })
                .ToList();

            foreach (
                var group in new[] { DeviceGroup.Both, DeviceGroup.Playback, DeviceGroup.Recording }
            )
            {
                profile.DefaultDeviceGroup = group;
                profileStore.Save(profile);
                var emptyMessage =
                    group == DeviceGroup.Both
                        ? "No visible recording devices"
                        : "No visible audio devices";
                Wait(
                    () =>
                        flyout!.FindFirst(
                            TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, emptyMessage)
                        )
                            is not null,
                    "The empty device group did not refresh."
                );
                var heading = group == DeviceGroup.Recording ? "Recording" : "Playback";
                Wait(
                    () =>
                        flyout!.FindFirst(
                            TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, heading)
                        )
                            is not null,
                    "The device group heading did not refresh."
                );
                if (group == DeviceGroup.Both)
                {
                    AssertBothGroups();
                    Assert.NotNull(
                        Find(flyout!, ControlType.ListItem, "No visible playback devices")
                    );
                }
            }

            var refreshedDefaults = ReadAudioDevices()
                .Where(device => device.Multimedia)
                .ToDictionary(device => device.Direction, device => device.Id);
            foreach (var direction in Enum.GetValues<Direction>())
            {
                Assert.Equal(
                    defaults.GetValueOrDefault(direction),
                    refreshedDefaults.GetValueOrDefault(direction)
                );
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

    private static AudioDevice[] ReadAudioDevices()
    {
        AudioDevice[] devices = [];
        Exception? failure = null;
        // Use a fresh COM apartment so UI Automation's error state cannot leak into audio calls.
        var thread = new Thread(() =>
        {
            try
            {
                using var audio = new AudioService();
                devices = Enum.GetValues<Direction>()
                    .SelectMany(direction => audio.List(direction))
                    .ToArray();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }

        return devices;
    }

    [UiFact]
    public void EverySettingsTabAndEveryLegacySkinOpensInActualWpfProcess()
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
        var start = new ProcessStartInfo(exe) { UseShellExecute = false };
        start.ArgumentList.Add("--settings");
        start.ArgumentList.Add("--exit-with-settings");
        start.ArgumentList.Add("--config");
        start.ArgumentList.Add(config);
        using var process = Process.Start(start)!;
        try
        {
            AutomationElement? settings = null;
            Wait(
                () => (settings = Window(process.Id, "AudioSwitch Settings")) is not null,
                "Settings window did not appear."
            );
            var transform = (TransformPattern)settings!.GetCurrentPattern(TransformPattern.Pattern);
            Assert.False(transform.Current.CanResize);
            var lastOption = Find(settings, ControlType.CheckBox, "Show OSD when scrolling");
            Assert.False(lastOption.Current.IsOffscreen);
            Assert.True(
                lastOption.Current.BoundingRectangle.Bottom
                    < Find(settings, ControlType.Button, "Apply").Current.BoundingRectangle.Top
            );
            var appearance = Find(settings!, ControlType.ComboBox);
            var group = settings!.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox)
            )[1];
            Assert.Equal(
                "Playback",
                ((SelectionPattern)group.GetCurrentPattern(SelectionPattern.Pattern))
                    .Current.GetSelection()
                    .Single()
                    .Current.Name
            );
            Choose(group, "Both");
            Assert.Equal(
                "System",
                ((SelectionPattern)appearance.GetCurrentPattern(SelectionPattern.Pattern))
                    .Current.GetSelection()
                    .Single()
                    .Current.Name
            );
            Choose(appearance, "Dark");
            Choose(appearance, "Light");
            Assert.False(File.Exists(config), "Previewing a theme unexpectedly saved settings.");
            var modifiers = Find(settings!, ControlType.Button, "Mouse-wheel modifiers");
            Invoke(modifiers);
            AutomationElement Modifier(string name) =>
                AutomationElement.RootElement.FindFirst(
                    TreeScope.Descendants,
                    new AndCondition(
                        new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id),
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.MenuItem
                        ),
                        new PropertyCondition(AutomationElement.NameProperty, name)
                    )
                ) ?? throw new InvalidOperationException("Missing modifier " + name);
            var ctrl = (TogglePattern)Modifier("Ctrl").GetCurrentPattern(TogglePattern.Pattern);
            ctrl.Toggle();
            Assert.Equal(ToggleState.On, ctrl.Current.ToggleState);
            var shift = (TogglePattern)Modifier("Shift").GetCurrentPattern(TogglePattern.Pattern);
            shift.Toggle();
            Assert.Equal(ToggleState.On, shift.Current.ToggleState);
            Invoke(modifiers);
            foreach (var name in new[] { "General", "Playback", "Recording", "Hotkeys", "OSD" })
            {
                Select(Find(settings!, ControlType.TabItem, name));
                if (name is "Playback" or "Recording")
                {
                    continue;
                }

                var bars = settings!.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(
                        AutomationElement.ControlTypeProperty,
                        ControlType.ScrollBar
                    )
                );
                Assert.All(
                    bars.Cast<AutomationElement>(),
                    bar => Assert.True(bar.Current.IsOffscreen)
                );
            }

            Select(Find(settings!, ControlType.TabItem, "Playback"));
            var deviceList = Find(settings!, ControlType.List);
            var deviceRows = deviceList.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem)
            );
            if (deviceRows.Count > 0)
            {
                Select(deviceRows[deviceRows.Count - 1]);
                var sliders = settings!.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Slider)
                );
                Assert.Equal(3, sliders.Count);
                var values = new[] { 210d, 75d, -20d };
                for (var i = 0; i < sliders.Count; i++)
                {
                    var range = (RangeValuePattern)
                        sliders[i].GetCurrentPattern(RangeValuePattern.Pattern);
                    range.SetValue(values[i]);
                    Assert.Equal(values[i], range.Current.Value);
                }

                var previews = settings
                    .FindAll(
                        TreeScope.Descendants,
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.Image
                        )
                    )
                    .Cast<AutomationElement>()
                    .Where(element => element.Current.Name.StartsWith("Tray icon "))
                    .ToArray();
                Assert.Equal(6, previews.Length);
                foreach (var iconPreview in previews)
                {
                    Assert.False(iconPreview.Current.IsOffscreen);
                    Assert.True(
                        settings.Current.BoundingRectangle.Contains(
                            iconPreview.Current.BoundingRectangle
                        )
                    );
                    Assert.True(
                        iconPreview.Current.BoundingRectangle.Bottom
                            <= sliders[0].Current.BoundingRectangle.Top
                    );
                }

                var saveDevice = Find(settings, ControlType.Button, "Save device");
                Assert.False(saveDevice.Current.IsOffscreen);
                Assert.True(
                    saveDevice.Current.BoundingRectangle.Bottom
                        < Find(settings, ControlType.Button, "Apply").Current.BoundingRectangle.Top
                );
                (
                    (TogglePattern)
                        Find(settings!, ControlType.CheckBox, "Use custom name")
                            .GetCurrentPattern(TogglePattern.Pattern)
                ).Toggle();
                (
                    (ValuePattern)
                        Find(settings!, ControlType.Edit).GetCurrentPattern(ValuePattern.Pattern)
                ).SetValue("000 Custom OSD device");
            }

            Select(Find(settings!, ControlType.TabItem, "OSD"));
            Invoke(Find(settings!, ControlType.Button, "Device preview"));
            if (deviceRows.Count > 0)
            {
                var deviceOsd = Window(process.Id, "AudioSwitch OSD")!;
                Assert.NotNull(Find(deviceOsd, ControlType.Text, "000 Custom OSD device"));
                Select(Find(settings!, ControlType.TabItem, "Playback"));
                var sortedRows = Find(settings!, ControlType.List)
                    .FindAll(
                        TreeScope.Children,
                        new PropertyCondition(
                            AutomationElement.ControlTypeProperty,
                            ControlType.ListItem
                        )
                    );
                Assert.NotNull(Find(sortedRows[0], ControlType.Text, "000 Custom OSD device"));
                Select(Find(settings!, ControlType.TabItem, "OSD"));
            }

            Wait(
                () => Window(process.Id, "AudioSwitch OSD") is not null,
                "Entering the OSD tab did not show a preview."
            );
            var combo = Find(settings!, ControlType.ComboBox);
            foreach (
                var skin in Directory
                    .GetDirectories(Path.Combine(Path.GetDirectoryName(exe)!, "Skins"))
                    .Select(Path.GetFileName)
            )
            {
                (
                    (ExpandCollapsePattern)combo.GetCurrentPattern(ExpandCollapsePattern.Pattern)
                ).Expand();
                AutomationElement? item = null;
                Wait(
                    () =>
                    {
                        item = combo.FindFirst(
                            TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, skin)
                        );
                        return item is not null;
                    },
                    "Missing skin " + skin
                );
                Select(item!);
                (
                    (ExpandCollapsePattern)combo.GetCurrentPattern(ExpandCollapsePattern.Pattern)
                ).Collapse();
                Invoke(Find(settings!, ControlType.Button, "Volume preview"));
                Wait(
                    () =>
                        Window(process.Id, "AudioSwitch OSD") is { } osd
                        && osd.Current.BoundingRectangle.Width > 20,
                    "Preview failed for " + skin
                );
                Invoke(Find(settings!, ControlType.Button, "Mute preview"));
                Invoke(Find(settings!, ControlType.Button, "Device preview"));
            }

            Select(Find(settings!, ControlType.TabItem, "General"));
            Wait(
                () => Window(process.Id, "AudioSwitch OSD") is null,
                "Leaving the OSD tab did not hide the preview."
            );
            Select(Find(settings!, ControlType.TabItem, "OSD"));
            var preview = Window(process.Id, "AudioSwitch OSD")!;
            var before = preview.Current.BoundingRectangle;
            Assert.True(
                SetWindowPos(
                    preview.Current.NativeWindowHandle,
                    0,
                    (int)before.Left + 35,
                    (int)before.Top + 30,
                    0,
                    0,
                    0x15
                )
            );
            Wait(
                () => Math.Abs(preview.Current.BoundingRectangle.Left - before.Left) > 5,
                "OSD did not move."
            );
            var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
            Assert.True(
                GetMonitorInfo(MonitorFromWindow(preview.Current.NativeWindowHandle, 2), ref info)
            );
            var bounds = preview.Current.BoundingRectangle;
            var taskbarX = (info.Monitor.Left + info.Monitor.Right - (int)bounds.Width) / 2;
            var taskbarY = info.Monitor.Bottom - (int)bounds.Height;
            if (info.Work.Top > info.Monitor.Top)
            {
                taskbarY = info.Monitor.Top;
            }
            else if (info.Work.Left > info.Monitor.Left)
            {
                taskbarX = info.Monitor.Left;
            }
            else if (info.Work.Right < info.Monitor.Right)
            {
                taskbarX = info.Monitor.Right - (int)bounds.Width;
            }

            Assert.True(
                SetWindowPos(preview.Current.NativeWindowHandle, 0, taskbarX, taskbarY, 0, 0, 0x15)
            );
            Wait(
                () => Math.Abs(preview.Current.BoundingRectangle.Top - taskbarY) <= 2,
                "OSD did not move onto the taskbar area."
            );
            Invoke(Find(settings!, ControlType.Button, "Volume preview"));
            Select(Find(settings!, ControlType.TabItem, "General"));
            Wait(
                () => Window(process.Id, "AudioSwitch OSD") is null,
                "OSD did not hide before position check."
            );
            Select(Find(settings!, ControlType.TabItem, "OSD"));
            Wait(
                () => Window(process.Id, "AudioSwitch OSD") is not null,
                "OSD did not reopen for position check."
            );
            Assert.InRange(Math.Abs(preview.Current.BoundingRectangle.Left - taskbarX), 0, 2);
            Assert.InRange(Math.Abs(preview.Current.BoundingRectangle.Top - taskbarY), 0, 2);
            Invoke(Find(settings!, ControlType.Button, "Apply"));
            Wait(() => File.Exists(config), "Apply did not persist settings.");
            var saved = new SettingsStore(config).Load();
            if (deviceRows.Count > 0)
            {
                var colored = Assert.Single(
                    saved.Devices,
                    device => device.CustomName == "000 Custom OSD device"
                );
                Assert.Equal(210, colored.Hue);
                Assert.Equal(75, colored.Saturation);
                Assert.Equal(-20, colored.Brightness);
            }
            Assert.Equal(AppTheme.Light, saved.Theme);
            Assert.Equal(DeviceGroup.Both, saved.DefaultDeviceGroup);
            Assert.Equal(ScrollKeys.LWin | ScrollKeys.Control | ScrollKeys.Shift, saved.ScrollKeys);
            Assert.InRange(Math.Abs(saved.Osd.Left - preview.Current.BoundingRectangle.Left), 0, 2);
            Assert.InRange(Math.Abs(saved.Osd.Top - preview.Current.BoundingRectangle.Top), 0, 2);
            var savedText = File.ReadAllText(config);
            Select(Find(settings!, ControlType.TabItem, "General"));
            Choose(Find(settings!, ControlType.ComboBox), "Dark");
            Invoke(Find(settings!, ControlType.Button, "Cancel"));
            Assert.True(process.WaitForExit(5000), "Tray did not shut down cleanly.");
            Assert.Equal(savedText, File.ReadAllText(config));
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
