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

using System.Runtime.InteropServices;
using AudioSwitch.CoreAudioApi.Interfaces;

namespace AudioSwitch.CoreAudioApi
{
    //Marked as internal, since on its own its no good
    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class _MMDeviceEnumerator
    {
    }

    //Small wrapper class
    internal class MMDeviceEnumerator
    {
        private readonly IMMDeviceEnumerator _realEnumerator = new _MMDeviceEnumerator() as IMMDeviceEnumerator;

        public MMDeviceCollection EnumerateAudioEndPoints(EDataFlow dataFlow, EDeviceState dwStateMask)
        {
            IMMDeviceCollection result;
            Marshal.ThrowExceptionForHR(_realEnumerator.EnumAudioEndpoints(dataFlow, dwStateMask, out result));
            return new MMDeviceCollection(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataFlow"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        /// <exception cref="COMException"></exception>
        /// <exception cref="DeviceNotFoundException">If there is no default device</exception>
        public MMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role)
        {
            try
            {
                IMMDevice _Device;
                Marshal.ThrowExceptionForHR(_realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out _Device));
                return new MMDevice(_Device);

            }
            catch (COMException comException) when (comException.ErrorCode == DeviceNotFoundException.E_NOTFOUND)
            {//if COMException will be used the same way in other methods consider create Exception Mapper instead of manual try/catch
                throw new DeviceNotFoundException("No devices found during default device detection", comException);
            }
        }

        public MMDevice GetDevice(string deviceId)
        {
            IMMDevice deviceFromId;
            Marshal.ThrowExceptionForHR(_realEnumerator.GetDevice(deviceId, out deviceFromId));
            return new MMDevice(deviceFromId);
        }

        /// <summary>
        /// Registers a client's notification callback interface.
        /// </summary>
        /// <param name="client">The <see cref="IMMNotificationClient"/> interface that the client is registering for notification callbacks.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        public void RegisterEndpointNotificationCallback(IMMNotificationClient client)
        {
            Marshal.ThrowExceptionForHR(_realEnumerator.RegisterEndpointNotificationCallback(client));
        }

        /// <summary>
        /// Deletes the registration of a notification interface that the client registered in a previous call
        /// to the <see cref="IMMDeviceEnumerator.RegisterEndpointNotificationCallback"/> method.
        /// </summary>
        /// <param name="client">A <see cref="IMMNotificationClient"/> interface that was previously registered for notification callbacks.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        public void UnregisterEndpointNotificationCallback(IMMNotificationClient client)
        {
            Marshal.ThrowExceptionForHR(_realEnumerator.UnregisterEndpointNotificationCallback(client));
        }
    }
}