/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Runtime.InteropServices;
using AudioSwitch.CoreAudioApi.Interfaces;

namespace AudioSwitch.CoreAudioApi
{
    internal class MMDevice
    {
        private readonly IMMDevice _RealDevice;
        private PropertyStore _PropertyStore;
        private AudioMeterInformation _AudioMeterInformation;
        private AudioEndpointVolume _AudioEndpointVolume;
        private AudioSessionManager _AudioSessionManager;

        private static Guid IID_IAudioMeterInformation = typeof(IAudioMeterInformation).GUID;
        private static Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
        private static Guid IID_IAudioSessionManager = typeof(IAudioSessionManager2).GUID;

        private PropertyStore GetPropertyInformation()
        {
            IPropertyStore propstore;
            Marshal.ThrowExceptionForHR(_RealDevice.OpenPropertyStore(EStgmAccess.STGM_READ, out propstore));
            return new PropertyStore(propstore);
        }

        private void GetAudioSessionManager()
        {
            object result;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioSessionManager, CLSCTX.ALL, IntPtr.Zero, out result));
            _AudioSessionManager = new AudioSessionManager(result as IAudioSessionManager2);
        }

        private void GetAudioMeterInformation()
        {
            object result;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioMeterInformation, CLSCTX.ALL, IntPtr.Zero, out result));
            _AudioMeterInformation = new AudioMeterInformation(result as IAudioMeterInformation);
        }

        private void GetAudioEndpointVolume()
        {
            object result;
            Marshal.ThrowExceptionForHR(_RealDevice.Activate(ref IID_IAudioEndpointVolume, CLSCTX.ALL, IntPtr.Zero, out result));
            _AudioEndpointVolume = new AudioEndpointVolume(result as IAudioEndpointVolume);
        }

        public AudioSessionManager AudioSessionManager
        {
            get
            {
                if (_AudioSessionManager == null)
                    GetAudioSessionManager();

                return _AudioSessionManager;
            }
        }

        public AudioMeterInformation AudioMeterInformation
        {
            get
            {
                if (_AudioMeterInformation == null)
                    GetAudioMeterInformation();

                return _AudioMeterInformation;
            }
        }

        public AudioEndpointVolume AudioEndpointVolume
        {
            get
            {
                if (_AudioEndpointVolume == null)
                    GetAudioEndpointVolume();

                return _AudioEndpointVolume;
            }
        }

        public string FriendlyName
        {
            get
            {
                if (_PropertyStore == null)
                    _PropertyStore = GetPropertyInformation();
                if (_PropertyStore.Contains(PKEY.PKEY_DeviceInterface_FriendlyName))
                    return (string)_PropertyStore[PKEY.PKEY_DeviceInterface_FriendlyName].PropVariant.GetValue();
                return "Unknown";
            }
        }

        public string IconPath
        {
            get
            {
                if (_PropertyStore == null)
                    _PropertyStore = GetPropertyInformation();
                if (_PropertyStore.Contains(PKEY.PKEY_DeviceClass_IconPath))
                    return (string)_PropertyStore[PKEY.PKEY_DeviceClass_IconPath].PropVariant.GetValue();
                return "Unknown";
            }
        }

        public string ID
        {
            get
            {
                string Result;
                Marshal.ThrowExceptionForHR(_RealDevice.GetId(out Result));
                return Result;
            }
        }

        internal MMDevice(IMMDevice realDevice)
        {
            _RealDevice = realDevice;
        }
    }
}
