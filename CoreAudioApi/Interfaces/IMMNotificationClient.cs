// -----------------------------------------
// SoundScribe (TM) and related software.
// 
// Copyright (C) 2007-2011 Vannatech
// http://www.vannatech.com
// All rights reserved.
// 
// This source code is subject to the MIT License.
// http://www.opensource.org/licenses/mit-license.php
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------

using System;
using System.Runtime.InteropServices;

namespace AudioSwitch.CoreAudioApi.Interfaces
{
    /// <summary>
    /// Provides notifications when an audio endpoint device is added or removed, when the state
    /// or properties of a device change, or when there is a change in the default role assigned to a device.
    /// </summary>
    /// <remarks>
    /// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd371417.aspx
    /// </remarks>
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
		/// <summary>
		/// Notifies the client that the default audio endpoint device for a particular role has changed.
		/// </summary>
		/// <param name="deviceId">The endpoint ID string that identifies the audio endpoint device.</param>
        /// <param name="newState">The <see cref="EDeviceState"/> constant that indicates the new state.</param>
        [PreserveSig]
		void OnDeviceStateChanged(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId,
            [MarshalAs(UnmanagedType.U4)] EDeviceState newState);

		/// <summary>
		/// Indicates that a new audio endpoint device has been added.
		/// </summary>
		/// <param name="deviceId">The endpoint ID string that identifies the audio endpoint device.</param>
        [PreserveSig]
        void OnDeviceAdded(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId);

		/// <summary>
		/// Indicates that an audio endpoint device has been removed.
		/// </summary>
		/// <param name="deviceId">The endpoint ID string that identifies the audio endpoint device.</param>
        [PreserveSig]
        void OnDeviceRemoved(
			[MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        /// <summary>
        /// Notifies the client that the default audio endpoint device for a particular role has changed.
        /// </summary>
        /// <param name="dataFlow">The data-flow direction of the endpoint device.</param>
        /// <param name="deviceRole">The device role of the audio endpoint device.</param>
        /// <param name="defaultDeviceId">The endpoint ID string that identifies the audio endpoint device.</param>
        [PreserveSig]
        void OnDefaultDeviceChanged(
			[MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
			[MarshalAs(UnmanagedType.I4)] ERole deviceRole,
            [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

        /// <summary>
        /// Indicates that the value of a property belonging to an audio endpoint device has changed.
        /// </summary>
        /// <param name="deviceId">The endpoint ID string that identifies the audio endpoint device.</param>
		/// <param name="propertyKey">A <see cref="PROPERTYKEY"/> that specifies the type of property.</param>
        [PreserveSig]
        void OnPropertyValueChanged(
            [MarshalAs(UnmanagedType.LPWStr)] string deviceId, PROPERTYKEY propertyKey);
    }
}
