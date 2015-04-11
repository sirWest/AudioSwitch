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
using AudioSwitch.CoreAudioApi.Interfaces;

namespace AudioSwitch.CoreAudioApi
{
    public delegate void DevEventDelegate(string devID);

	/// <summary>
	/// Used for testing endpoint notification methods.
	/// </summary>
	public class MMDeviceNotifyClient : IMMNotificationClient
	{
        public DevEventDelegate DefaultChanged;
	    public DevEventDelegate DeviceAdded;
	    public DevEventDelegate DeviceRemoved;

		public void OnDefaultDeviceChanged(EDataFlow dataFlow, ERole deviceRole, string defaultDeviceId)
		{
		    if (DefaultChanged != null)
		        DefaultChanged(defaultDeviceId);
		}

        public void OnDeviceStateChanged(string deviceId, EDeviceState newState)
	    {
            if (newState == EDeviceState.Unplugged || newState == EDeviceState.Disabled || newState == EDeviceState.NotPresent)
            {
                if (DeviceRemoved != null)
                    DeviceRemoved(deviceId);
            }
            else if (newState == EDeviceState.Active)
                if (DeviceAdded != null)
                    DeviceAdded(deviceId);
	    }

	    public void OnDeviceAdded(string deviceId)
	    {
	        if (DeviceAdded != null)
	            DeviceAdded(deviceId);
	    }

		public void OnDeviceRemoved(string deviceId)
		{
            if (DeviceRemoved != null)
                DeviceRemoved(deviceId);
		}

		public void OnPropertyValueChanged(string deviceId, PROPERTYKEY propertyKey)
		{

		}
	}
}