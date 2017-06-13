using System;
using System.Runtime.InteropServices;

namespace AudioSwitch.CoreAudioApi
{
    //Inheriting COMException for backward compatibility (the same for HResult in ctor)
    public class DeviceNotFoundException : COMException
    {
        // ReSharper disable once InconsistentNaming
        internal const int E_NOTFOUND = -2147023728;

        internal DeviceNotFoundException(string message, Exception inner) : base(message, inner)
        {
            HResult = E_NOTFOUND;//https://msdn.microsoft.com/en-us/library/windows/desktop/dd371401(v=vs.85).aspx
        }
    }
}