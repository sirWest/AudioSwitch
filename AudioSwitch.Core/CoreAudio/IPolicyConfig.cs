using System.Runtime.InteropServices;

namespace AudioSwitch.Core.CoreAudio;

// Undocumented Windows policy interface. Keep the complete vtable order, even for unused slots.
[
    ComImport,
    Guid("F8679F50-850A-41CF-9C72-430F290290C8"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
]
internal interface IPolicyConfig
{
    [PreserveSig]
    int GetMixFormat(IntPtr id, IntPtr format);

    [PreserveSig]
    int GetDeviceFormat(IntPtr id, int flag, IntPtr format);

    [PreserveSig]
    int ResetDeviceFormat(IntPtr id);

    [PreserveSig]
    int SetDeviceFormat(IntPtr id, IntPtr endpoint, IntPtr mix);

    [PreserveSig]
    int GetProcessingPeriod(IntPtr id, int flag, IntPtr period, IntPtr minimum);

    [PreserveSig]
    int SetProcessingPeriod(IntPtr id, IntPtr period);

    [PreserveSig]
    int GetShareMode(IntPtr id, IntPtr mode);

    [PreserveSig]
    int SetShareMode(IntPtr id, IntPtr mode);

    [PreserveSig]
    int GetPropertyValue(IntPtr id, int store, IntPtr key, IntPtr value);

    [PreserveSig]
    int SetPropertyValue(IntPtr id, int store, IntPtr key, IntPtr value);

    [PreserveSig]
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string id, Role role);

    [PreserveSig]
    int SetEndpointVisibility(IntPtr id, int visible);
}
