using System;
using System.Runtime.InteropServices;
using AudioSwitch.Controls;
using AudioSwitch.CoreAudioApi.Interfaces;

namespace AudioSwitch.CoreAudioApi
{
    delegate void DeviceStateChanged();

    internal class VolEventsHandler : IAudioSessionEvents
    {
        private readonly VolumeBar volumebar;

        public VolEventsHandler(VolumeBar VolumeBar)
        {
            volumebar = VolumeBar;
        }

        delegate void MethodWithFloatArg(float Volume, bool Mute);
        

        private void SafeSet(float Volume, bool Mute)
        {
            if (volumebar.InvokeRequired) 
                volumebar.Invoke(new MethodWithFloatArg(SafeSet), Volume, Mute);
            else
            {
                volumebar.Value = Volume;
                volumebar.Mute = Mute;
                if (volumebar.VolumeMuteChanged != null)
                    volumebar.VolumeMuteChanged(this, null);
            }
        }

        //private void DevChanged()
        //{
        //    if (volumebar.InvokeRequired)
        //        volumebar.Invoke(new DeviceStateChanged(DevChanged));
        //    else
        //    {
        //        volumebar.DeviceStateChanged(EDataFlow.eAll);
        //        if (volumebar.VolumeMuteChanged != null)
        //            volumebar.VolumeMuteChanged(this, null);
        //    }
        //}

        [PreserveSig]
        public int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, Guid EventContext) { return 0; }
        [PreserveSig]
        public int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, Guid EventContext) { return 0; }

        [PreserveSig]
        public int OnSimpleVolumeChanged(float newVolume, bool newMute, Guid EventContext)
        {
            SafeSet(newVolume, newMute);
            return 0;
        }
        [PreserveSig]
        public int OnChannelVolumeChanged(UInt32 ChannelCount, IntPtr NewChannelVolumeArray, UInt32 ChangedChannel, Guid EventContext) { return 0; }
        [PreserveSig]
        public int OnGroupingParamChanged(Guid NewGroupingParam, Guid EventContext) { return 0; }

        [PreserveSig]
        public int OnStateChanged(AudioSessionState NewState)
        {
            //DevChanged();
            return 0;
        }
        [PreserveSig]
        public int OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) { return 0; }
    }
}
