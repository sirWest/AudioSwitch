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
	/// Specifies the FMTID/PID identifier that programmatically identifies a property.
	/// </summary>
	/// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/bb773381.aspx
	/// Note: This item is external to CoreAudio API, and is defined in the Windows Property System API.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential)]
	public struct PROPERTYKEY
	{
		/// <summary>
		/// A unique GUID for the property.
		/// </summary>
		public Guid fmtid;

		/// <summary>
		/// A property identifier (PID).
		/// </summary>
		public int pid;
	}
}
