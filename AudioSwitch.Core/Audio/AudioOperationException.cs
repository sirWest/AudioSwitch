using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AudioSwitch.Core.Audio;

public sealed class AudioOperationException : Exception
{
    internal AudioOperationException(string operation, Exception cause)
        : base(
            $"Windows audio could not {operation} (0x{cause.HResult:X8}): {cause.Message}",
            cause
        )
    {
        HResult = cause.HResult;
    }

    public static bool IsDeviceFailure(Exception exception) =>
        exception is AudioOperationException or COMException or InvalidCastException
        || AudioUnavailableException.IsUnavailable(exception);

    internal static void Check(
        int result,
        [CallerMemberName] string operation = "access the device"
    )
    {
        if (result < 0)
        {
            // Keep native failures identifiable even when .NET maps them to ArgumentException,
            // FileNotFoundException, etc. Do not hide unrelated managed programming errors.
            throw new AudioOperationException(operation, Marshal.GetExceptionForHR(result, -1)!);
        }
    }
}
