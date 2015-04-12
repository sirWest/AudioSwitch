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
        static Mutex mutex = new Mutex(true, "{579A9A19-7AE5-42CD-8147-E587F5C9DD50}");

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        internal static FormOSD frmOSD;
        private static FormSwitcher formSwitcher;
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

                    var cmdArgsJoined = string.Join(" ", args);
                    var cmdArgs = cmdArgsJoined.Split('-');

                    var hotkeyFunction = HotkeyFunction.SwitchPlaybackDevice;
                    var modifiers = HotModifierKeys.LWin;
                    var hotKey = Keys.LWin;
                    var rType = settings.DefaultDataFlow;
                    var keysOK = 0;

                    foreach (var arg in cmdArgs)
                    {
                        if (string.IsNullOrWhiteSpace(arg))
                            continue;

                        var purecmd = arg.Length > 1 ? arg.Substring(1, arg.Length - 1).Trim() : "";

                        switch (arg.Substring(0, 1))
                        {
                            case "m":
                                if (!Enum.TryParse(purecmd, true, out modifiers))
                                {
                                    Console.WriteLine("Error reading modifier key(s)!");
                                    return;
                                }
                                keysOK++;
                                break;

                            case "k":
                                if (!Enum.TryParse(purecmd, true, out hotKey))
                                {
                                    Console.WriteLine("Error reading hot key!");
                                    return;
                                }
                                keysOK++;
                                break;

                            case "f":
                                if (!Enum.TryParse(purecmd, true, out hotkeyFunction))
                                {
                                    Console.WriteLine("Error reading function name!");
                                    return;
                                }
                                keysOK++;
                                break;

                            case "i":
                                var devID = int.Parse(purecmd);
                                EndPoints.RefreshDeviceList(rType);
                                if (devID <= EndPoints.DeviceNames.Count - 1)
                                    EndPoints.SetDefaultDevice(devID);
                                break;

                            case "r":
                                switch (purecmd.ToLower())
                                {
                                    case "playback":
                                        rType = EDataFlow.eRender;
                                        break;
                                    case "recording":
                                        rType = EDataFlow.eCapture;
                                        break;
                                }

                                if (rType == EDataFlow.eAll)
                                {
                                    Console.WriteLine("Error reading render type!");
                                    return;
                                }
                                break;

                            case "l":
                                Console.WriteLine(" Devices available:");
                                EndPoints.RefreshDeviceList(rType);

                                for (var i = 0; i < EndPoints.DeviceNames.Count; i++)
                                    Console.WriteLine(i == EndPoints.DefaultDeviceID ? " <{0}> {1}" : "  {0}  {1}", i, EndPoints.DeviceNames[i]);
                                break;

                            case "s":
                                Settings.settingsxml = purecmd;
                                settings = Settings.Load();
                                rType = settings.DefaultDataFlow;
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

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                frmOSD = new FormOSD();
                formSwitcher = new FormSwitcher();
                Application.Run();
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
                MessageBox.Show("An unexpected error has occured - AudioSwitch will now close :(" + Environment.NewLine +
                                "Error messages are written to a log file 'ErrorLog.txt' in the installation folder. " + Environment.NewLine + 
                                "Please create an issue ticket in Google Code page with the contents of the log file.",
                                "AudioSwitch - Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}

