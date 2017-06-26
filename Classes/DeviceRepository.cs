using AudioSwitch.CoreAudioApi;
using System.Collections.Generic;

namespace AudioSwitch.Classes
{
    static class DeviceRepository
    {
        static DeviceRepository()
        {
            DeviceEnumerator = new MMDeviceEnumerator();
        }

        private static readonly MMDeviceEnumerator DeviceEnumerator;

        internal static IEnumerable<Device> FindAll()
        {
            var deviceList = new List<Device>();
            deviceList.AddRange(CreateDeviceList(EDataFlow.eCapture));
            deviceList.AddRange(CreateDeviceList(EDataFlow.eRender));
            return deviceList;
        }

        internal static List<Device> FindCaptureDevices()
        {
            var deviceList = new List<Device>();
            deviceList.AddRange(CreateDeviceList(EDataFlow.eCapture));
            return deviceList;
        }

        internal static List<Device> FindPlayBackDevices()
        {
            var deviceList = new List<Device>();
            deviceList.AddRange(CreateDeviceList(EDataFlow.eRender));
            return deviceList;
        }
        private static IEnumerable<Device> CreateDeviceList(EDataFlow dataFlow)
        {
            var devices = DeviceEnumerator.EnumerateAudioEndPoints(dataFlow, EDeviceState.Active);
            var count = devices.Count;
            var deviceList = new List<Device>();
            for (var i = 0; i < count; i++)
            {
                var device = CreateDevice(devices[i], dataFlow);
                if (device != null)
                {
                    deviceList.Add(device);
                }
            }
            return deviceList;
        }

        private static Device CreateDevice(MMDevice mmDevice, EDataFlow dataFlow)
        {
            var devId = mmDevice.ID;
            var devSettings = Program.settings.Device.Find(x => x.DeviceID == devId);
            var device = new Device
            {
                MMDevice = mmDevice,
                DataFlow = dataFlow
            };
            return device;
        }
    }
}
