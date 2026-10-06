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
// Modified for AudioSwitch: consolidated declarations, corrected pointer/BOOL
// marshaling, HRESULT callbacks, and architecture-correct PROPVARIANT storage.
using System.Runtime.InteropServices;

namespace AudioSwitch.Core.CoreAudio;

public enum DataFlow
{
    Render,
    Capture,
    All,
}

public enum Role
{
    Console,
    Multimedia,
    Communications,
}

[Flags]
public enum DeviceState
{
    Active = 1,
    Disabled = 2,
    NotPresent = 4,
    Unplugged = 8,
}

[StructLayout(LayoutKind.Sequential)]
public struct PropertyKey(Guid format, uint id)
{
    public Guid Format = format;
    public uint Id = id;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeBlob
{
    public uint Count;
    public nint Data;
}

[StructLayout(LayoutKind.Explicit)]
internal struct PropVariant
{
    [FieldOffset(0)]
    public ushort Type;

    [FieldOffset(8)]
    public nint Pointer;

    // Largest native union member: 16 bytes on x64, 8 bytes on x86.
    [FieldOffset(8)]
    private NativeBlob blob;
    internal string? String => Type == 31 ? Marshal.PtrToStringUni(Pointer) : null;

    [DllImport("ole32.dll")]
    internal static extern int PropVariantClear(ref PropVariant value);
}

[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class EnumeratorObject { }

[
    ComImport,
    Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IMMDeviceEnumerator
{
    [PreserveSig]
    int EnumAudioEndpoints(DataFlow flow, DeviceState mask, out IMMDeviceCollection devices);

    [PreserveSig]
    int GetDefaultAudioEndpoint(DataFlow flow, Role role, out IMMDevice device);

    [PreserveSig]
    int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);

    [PreserveSig]
    int RegisterEndpointNotificationCallback(IMMNotificationClient client);

    [PreserveSig]
    int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
}

[
    ComImport,
    Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IMMDeviceCollection
{
    [PreserveSig]
    int GetCount(out uint count);

    [PreserveSig]
    int Item(uint index, out IMMDevice device);
}

[
    ComImport,
    Guid("D666063F-1587-4E43-81F1-B948E807363F"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IMMDevice
{
    [PreserveSig]
    int Activate(
        ref Guid iid,
        uint context,
        nint parameters,
        [MarshalAs(UnmanagedType.IUnknown)] out object instance
    );

    [PreserveSig]
    int OpenPropertyStore(uint access, out IPropertyStore store);

    [PreserveSig]
    int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

    [PreserveSig]
    int GetState(out DeviceState state);
}

[
    ComImport,
    Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out uint count);

    [PreserveSig]
    int GetAt(uint index, out PropertyKey key);

    [PreserveSig]
    int GetValue(ref PropertyKey key, out PropVariant value);

    [PreserveSig]
    int SetValue(ref PropertyKey key, ref PropVariant value);

    [PreserveSig]
    int Commit();
}

[
    ComImport,
    Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IAudioEndpointVolume
{
    [PreserveSig]
    int RegisterControlChangeNotify(IAudioEndpointVolumeCallback callback);

    [PreserveSig]
    int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback callback);

    [PreserveSig]
    int GetChannelCount(out uint count);

    [PreserveSig]
    int SetMasterVolumeLevel(float value, ref Guid context);

    [PreserveSig]
    int SetMasterVolumeLevelScalar(float value, ref Guid context);

    [PreserveSig]
    int GetMasterVolumeLevel(out float value);

    [PreserveSig]
    int GetMasterVolumeLevelScalar(out float value);

    [PreserveSig]
    int SetChannelVolumeLevel(uint channel, float value, ref Guid context);

    [PreserveSig]
    int SetChannelVolumeLevelScalar(uint channel, float value, ref Guid context);

    [PreserveSig]
    int GetChannelVolumeLevel(uint channel, out float value);

    [PreserveSig]
    int GetChannelVolumeLevelScalar(uint channel, out float value);

    [PreserveSig]
    int SetMute([MarshalAs(UnmanagedType.Bool)] bool value, ref Guid context);

    [PreserveSig]
    int GetMute([MarshalAs(UnmanagedType.Bool)] out bool value);

    [PreserveSig]
    int GetVolumeStepInfo(out uint step, out uint count);

    [PreserveSig]
    int VolumeStepUp(ref Guid context);

    [PreserveSig]
    int VolumeStepDown(ref Guid context);

    [PreserveSig]
    int QueryHardwareSupport(out uint mask);

    [PreserveSig]
    int GetVolumeRange(out float min, out float max, out float increment);
}

[
    ComImport,
    Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IAudioMeterInformation
{
    [PreserveSig]
    int GetPeakValue(out float peak);

    [PreserveSig]
    int GetMeteringChannelCount(out uint count);

    [PreserveSig]
    int GetChannelsPeakValues(
        uint count,
        [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] float[] peaks
    );

    [PreserveSig]
    int QueryHardwareSupport(out uint mask);
}

[
    ComVisible(true),
    Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
public interface IAudioEndpointVolumeCallback
{
    [PreserveSig]
    int OnNotify(nint data);
}

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
// Modified for AudioSwitch: explicit COM visibility and HRESULT return values.
[
    ComVisible(true),
    Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
public interface IMMNotificationClient
{
    [PreserveSig]
    int OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string id, DeviceState state);

    [PreserveSig]
    int OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string id);

    [PreserveSig]
    int OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string id);

    [PreserveSig]
    int OnDefaultDeviceChanged(
        DataFlow flow,
        Role role,
        [MarshalAs(UnmanagedType.LPWStr)] string? id
    );

    [PreserveSig]
    int OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string id, PropertyKey key);
}
