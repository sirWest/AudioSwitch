using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AudioSwitch.CoreAudioApi;
using AudioSwitch.Forms;

namespace AudioSwitch.Classes
{
    internal static class Program
    {
        static readonly Mutex mutex = new Mutex(true, "{579A9A19-7AE5-42CD-8147-E587F5C9DD50}");

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        internal static FormOSD frmOSD;
        private static bool isConsole;
        internal static Settings settings;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                settings = Settings.Load();

                if (args.Length > 0)
                {
                    if (!AttachConsole(-1))
                        AllocConsole();
                    isConsole = true;
                    Console.WriteLine();

                    var hotkeyFunction = HotkeyFunction.NextPlaybackDevice;
                    var modifiers = HotModifierKeys.LWin;
                    var hotKey = Keys.LWin;
                    var rType = settings.DefaultDataFlow;
                    var keysOK = 0;

                    for (var index = 0; index < args.Length; index++)
                    {
                        var arg = args[index];
                        if (string.IsNullOrWhiteSpace(arg))
                            continue;

                        switch (arg.Substring(1, arg.Length - 1))
                        {
                            case "help":
                            case "h":
                            case "?":
                                Console.WriteLine();
                                Console.WriteLine("AudioSwitch v2.0 command-line help");
                                Console.WriteLine("----------------------------------");
                                Console.WriteLine("Available commands:");
                                Console.WriteLine("  /i - switch to input devices for the command. Default taken from settings.");
                                Console.WriteLine("  /o - switch to output devices for the command. Default taken from settings.");
                                Console.WriteLine();
                                Console.WriteLine("  /l - list all available devices of the selected type, < > is current default.");
                                Console.WriteLine("  /s - set a device as default: number for index, string for name.");
                                Console.WriteLine();
                                Console.WriteLine("  /m - set modifier keys for a new hotkey, separated by commas. Case sensitive.");
                                Console.WriteLine("  /k - set a key for a new hotkey. Case sensitive.");
                                Console.WriteLine("  /f - choose a function for the new hotkey. Case sensitive.");
                                Console.WriteLine();
                                Console.WriteLine("  /m, /k, /f (blank) - shows all possible values for the parameter.");
                                break;

                            case "i":
                                rType = EDataFlow.eCapture;
                                Console.WriteLine("Device group changed to input devices.");
                                break;

                            case "o":
                                rType = EDataFlow.eRender;
                                Console.WriteLine("Device group changed to output devices.");
                                break;

                            case "l":
                                Console.WriteLine();
                                Console.WriteLine(" Devices available:");
                                EndPoints.RefreshDeviceList(rType);

                                var i = 0;
                                foreach (var device in EndPoints.DeviceNames.Values)
                                    Console.WriteLine(device == EndPoints.DefaultDeviceName ? " <{0}> {1}" : "  {0}  {1}", i++, device);
                                Console.WriteLine();
                                break;

                            case "s":
                                index++;
                                int devID;

                                if (int.TryParse(args[index], out devID))
                                {
                                    EndPoints.RefreshDeviceList(rType);
                                    if (devID <= EndPoints.DeviceNames.Count - 1)
                                    {
                                        EndPoints.SetDefaultDeviceByID(devID);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error changing device!");
                                        return;
                                    }
                                }
                                else
                                {
                                    EndPoints.RefreshDeviceList(rType);
                                    if (!EndPoints.SetDefaultDeviceByName(args[index]))
                                    {
                                        Console.WriteLine("Error changing device!");
                                        return;
                                    }
                                }
                                Console.WriteLine("Device changed to \"" + EndPoints.DefaultDeviceName + "\"");
                                break;

                            case "m":
                                if (index == args.Length - 1)
                                {
                                    Console.WriteLine("Modifier keys:");
                                    Console.WriteLine(string.Join(", ", Enum.GetNames(typeof(HotModifierKeys))));
                                    return;
                                }
                                index++;
                                if (!Enum.TryParse(args[index], true, out modifiers))
                                {
                                    Console.WriteLine("Error reading modifier key(s)!");
                                    return;
                                }
                                Console.WriteLine("Modifier key(s) set to " + modifiers);
                                keysOK++;
                                break;

                            case "k":
                                if (index == args.Length - 1)
                                {
                                    Console.WriteLine("Keys:");
                                    Console.WriteLine(string.Join(", ", Enum.GetNames(typeof(Keys))));
                                    return;
                                }
                                index++;
                                if (!Enum.TryParse(args[index], true, out hotKey))
                                {
                                    Console.WriteLine("Error reading hot key!");
                                    return;
                                }
                                Console.WriteLine("Hot key set to " + hotKey);
                                keysOK++;
                                break;

                            case "f":
                                if (index == args.Length - 1)
                                {
                                    Console.WriteLine("Function names:");
                                    Console.WriteLine(string.Join(", ", Enum.GetNames(typeof(HotkeyFunction))));
                                    return;
                                }
                                index++;
                                if (!Enum.TryParse(args[index], true, out hotkeyFunction))
                                {
                                    Console.WriteLine("Error reading function name!");
                                    return;
                                }
                                Console.WriteLine("Hot key function set to " + hotkeyFunction);
                                keysOK++;
                                break;
                        }
                    }

                    if (keysOK == 3)
                    {
                        var hkey = GlobalHotkeys.AddOrFind(hotkeyFunction);
                        hkey.ModifierKeys = modifiers;
                        hkey.HotKey = hotKey;
                        Console.WriteLine("Hot key saved:  {0} => {1} + {2}", hotkeyFunction, modifiers, hotKey);
                        settings.Save();
                    }

                    return;
                }

                if (mutex.WaitOne(0, false))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    frmOSD = new FormOSD();
                    var formSwitcher = new FormSwitcher();
                    Application.Run();
                    mutex.Close();
                }
            }
            finally
            {
                if (isConsole)
                {
                    SendKeys.SendWait("{ENTER}");
                    FreeConsole();
                }
            }
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = (Exception)e.ExceptionObject;
                using (var w = File.AppendText("ErrorLog.txt"))
                {
                    w.WriteLine("Log Entry: {0}", DateTime.Now);
                    w.WriteLine("");
                    w.WriteLine(ex.ToString());
                    w.WriteLine("-------------------------------");
                    w.Flush();
                    w.Close();
                }
                MessageBox.Show("An unexpected error has occurred - AudioSwitch will now close :(" + Environment.NewLine +
                                "Error messages are written to a log file 'ErrorLog.txt' in the installation folder. " + Environment.NewLine + 
                                "Please create an issue in GitHub issue tracker page with the contents of the log file.",
                                "AudioSwitch - Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}

