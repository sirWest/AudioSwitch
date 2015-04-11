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

namespace AudioSwitch.CoreAudioApi.Interfaces
{
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        /// <summary>
        /// Generates a collection of audio endpoint devices that meet the specified criteria.
        /// </summary>
        /// <param name="dataFlow">The <see cref="EDataFlow"/> direction for the endpoint devices in the collection.</param>
        /// <param name="stateMask">One or more <see cref="EDeviceState"/> constants that indicate the state of the endpoints in the collection.</param>
        /// <param name="devices">The <see cref="IMMDeviceCollection"/> interface of the device-collection object.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int EnumAudioEndpoints(
            [In] [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In] [MarshalAs(UnmanagedType.U4)] EDeviceState stateMask,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDeviceCollection devices);

        /// <summary>
        /// Retrieves the default audio endpoint for the specified data-flow direction and role.
        /// </summary>
        /// <param name="dataFlow">The <see cref="EDataFlow"/> direction for the endpoint device.</param>
        /// <param name="role">The <see cref="ERole"/> of the endpoint device.</param>
        /// <param name="device">The <see cref="IMMDevice"/> interface of the default audio endpoint device.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetDefaultAudioEndpoint(
            [In] [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In] [MarshalAs(UnmanagedType.I4)] ERole role,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);

        /// <summary>
        /// Retrieves an endpoint device that is specified by an endpoint device-identification string.
        /// </summary>
        /// <param name="endpointId">The endpoint ID.</param>
        /// <param name="device">The <see cref="IMMDevice"/> interface for the specified device.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetDevice(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string endpointId,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IMMDevice device);

        /// <summary>
        /// Registers a client's notification callback interface.
        /// </summary>
        /// <param name="client">The <see cref="IMMNotificationClient"/> interface that the client is registering for notification callbacks.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int RegisterEndpointNotificationCallback(
            [In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client);

        /// <summary>
        /// Deletes the registration of a notification interface that the client registered in a previous call
        /// to the <see cref="IMMDeviceEnumerator.RegisterEndpointNotificationCallback"/> method.
        /// </summary>
        /// <param name="client">A <see cref="IMMNotificationClient"/> interface that was previously registered for notification callbacks.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int UnregisterEndpointNotificationCallback(
            [In] [MarshalAs(UnmanagedType.Interface)] IMMNotificationClient client);
    }
}
