using AudioSwitch.CoreAudioApi;
using AudioSwitch.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using static AudioSwitch.Classes.Settings;

namespace AudioSwitch.Classes
{
    class StartupDeviceTask
    {
        public void Run()
        {
            var devices = DeviceRepository.FindAll();
            List<DeviceChanges> devicesSet = new List<DeviceChanges>();
            foreach (var dev in devices)
            {
                var devID = dev.MMDevice.ID;
                var devSettings = Program.settings.Device.Find(x => x.DeviceID == devID);
                if (devSettings != null) { 
                    if (devSettings.DefaultMultimediaDevice)
                    {
                        var changes = switchDevice(devSettings, dev, ERole.eMultimedia);
                        if(changes != null)
                        {
                            devicesSet.Add(changes);
                        }
                    }
                    if (devSettings.DefaultCommunicationsDevice)
                    {
                        var changes = switchDevice(devSettings, dev, ERole.eCommunications);
                        if (changes != null)
                        {
                            devicesSet.Add(changes);
                        }
                    }
                }
            }
            if (devicesSet.Count > 0) {
                ShowBalloonTip(devicesSet);
            }

        }

        private DeviceChanges switchDevice(CDevice devSettings, Device dev, CoreAudioApi.ERole role)
        {
            var defaultDevice = EndPoints.GetDefaultMMDevice(dev.DataFlow, role);
            if (defaultDevice == null || defaultDevice.ID == null || !defaultDevice.ID.Equals(devSettings.DeviceID))
            {
                EndPoints.SetDefaultDevice(devSettings.DeviceID, role);
                string deviceName = devSettings.UseCustomName && devSettings.CustomName != null && devSettings.CustomName != "" ? devSettings.CustomName : dev.MMDevice.FriendlyName;
                return new DeviceChanges { deviceName = deviceName, role = role, dataflow = dev.DataFlow };
            }
            return null;
        }

        private void ShowBalloonTip(List<DeviceChanges> deviceNames)
        {
            var attributes = typeof(Program).GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
            var assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;
            var text = String.Join("\n", deviceNames.Select(n => "Switching to " + 
            (EDataFlow.eRender.Equals(n.dataflow) ? "Multimedia" : "Recording")
            + " \"" + n.deviceName + "\" as default " + 
            (ERole.eMultimedia.Equals(n.role) ? "multimedia" : "communications") + " device."));
            var notification = new System.Windows.Forms.NotifyIcon()
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipTitle = assemblyTitleAttribute.Title,
                BalloonTipText = text,

            };
            notification.BalloonTipClicked += new EventHandler(TrayNotifyIcon_BalloonClick);
            notification.ShowBalloonTip(2);
            DisposeBalloon(notification).ContinueWith(t => Console.WriteLine(t.Exception),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private void TrayNotifyIcon_BalloonClick(object sender, EventArgs e)
        {
            var cfg = new FormSettings();
            cfg.ShowDialog();
        }


        private async Task DisposeBalloon(System.Windows.Forms.NotifyIcon notification)
        {
            System.Threading.Thread.Sleep(2000);
            notification.Dispose();
        }

        internal class DeviceChanges
        {
            internal string deviceName { get; set; }
            internal ERole role { get; set; }
            internal EDataFlow dataflow { get; set; }
        }
    }
}
