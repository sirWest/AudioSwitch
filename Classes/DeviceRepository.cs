using AudioSwitch.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSwitch.Classes
{
    class DeviceRepository
    {

        static DeviceRepository()
        {
            DeviceEnumerator = new MMDeviceEnumerator();
            //NotifyClient = new MMDeviceNotifyClient();
            //DeviceEnumerator.RegisterEndpointNotificationCallback(NotifyClient);
        }
        internal static readonly MMDeviceEnumerator DeviceEnumerator;

        internal static List<Device> FindAll()
        {
            List<Device> deviceList = new List<Device>();
            deviceList.AddRange(createDeviceList(EDataFlow.eCapture));
            deviceList.AddRange(createDeviceList(EDataFlow.eRender));
            return deviceList;
        }

        internal static List<Device> FindCaptureDevices()
        {
            List<Device> deviceList = new List<Device>();
            deviceList.AddRange(createDeviceList(EDataFlow.eCapture));
            return deviceList;
        }

        internal static List<Device> FindPlayBackDevices()
        {
            List<Device> deviceList = new List<Device>();
            deviceList.AddRange(createDeviceList(EDataFlow.eRender));
            return deviceList;
        }
        private static List<Device> createDeviceList(EDataFlow dataFlow)
        {
            var devices = DeviceEnumerator.EnumerateAudioEndPoints(dataFlow, EDeviceState.Active);
            var count = devices.Count;
            List<Device> deviceList = new List<Device>();
            for (var i = 0; i < count; i++)
            {
                var device = createDevice(devices[i], dataFlow);
                if (device != null)
                {
                    deviceList.Add(device);
                }
            }
            return deviceList;
        }

        private static Device createDevice(MMDevice mmDevice, EDataFlow dataFlow)
        {
            var devId = (string)mmDevice.ID;
            var devSettings = Program.settings.Device.Find(x => x.DeviceID == devId);
            if (devSettings == null || !devSettings.HideFromList)
            {
                if (devSettings == null)
                {
                    devSettings = new Settings.CDevice
                    {
                        DeviceID = devId,
                        HideFromList = false,
                        Brightness = 0,
                        Hue = 0,
                        Saturation = 0,
                        UseCustomName = false,
                        CustomName = "",
                        DefaultMultimediaDevice = false,
                        DefaultCommunicationsDevice = false
                    };
                }
                var device = new Device
                {
                    MMDevice = mmDevice,
                    DataFlow = dataFlow
                };
                return device;
            }
            return null;
        }
    }
}
