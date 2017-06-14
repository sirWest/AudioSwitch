using AudioSwitch.CoreAudioApi;
using AudioSwitch.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioSwitch.Classes
{
    class StartupDeviceTask
    {
        public void Run()
        {
            var devices = DeviceRepository.FindAll();
            var devicesSet = new List<DeviceChanges>();
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

        private DeviceChanges switchDevice(Settings.CDevice devSettings, Device dev, ERole role)
        {
            var defaultDevice = EndPoints.GetDefaultMMDevice(dev.DataFlow, role);
            if (defaultDevice?.ID == null || !defaultDevice.ID.Equals(devSettings.DeviceID))
            {
                EndPoints.SetDefaultDevice(devSettings.DeviceID, role);
                string deviceName = devSettings.UseCustomName && !string.IsNullOrEmpty(devSettings.CustomName) ? devSettings.CustomName : dev.MMDevice.FriendlyName;
                return new DeviceChanges { deviceName = deviceName, role = role, dataflow = dev.DataFlow };
            }
            return null;
        }

        private void ShowBalloonTip(IEnumerable<DeviceChanges> deviceNames)
        {
            var text = string.Join("\n", deviceNames.Select(n => "Switching to " + 
            (EDataFlow.eRender.Equals(n.dataflow) ? "Multimedia" : "Recording")
            + " \"" + n.deviceName + "\" as default " + 
            (ERole.eMultimedia.Equals(n.role) ? "multimedia" : "communications") + " device"));
            using (var notification = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipTitle = typeof(Program).Assembly.GetName().Name,
                BalloonTipText = text

            })
            {
                notification.BalloonTipClicked += TrayNotifyIcon_BalloonClick;
                notification.ShowBalloonTip(2);
            }
        }

        private void TrayNotifyIcon_BalloonClick(object sender, EventArgs e)
        {
            var cfg = new FormSettings();
            cfg.ShowDialog();
        }

        private class DeviceChanges
        {
            internal string deviceName { get; set; }
            internal ERole role { get; set; }
            internal EDataFlow dataflow { get; set; }
        }
    }
}
