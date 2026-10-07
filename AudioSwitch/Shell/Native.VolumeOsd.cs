using System.Runtime.InteropServices;
using AudioSwitch.Core.Audio;

namespace AudioSwitch.Shell;

internal static partial class Native
{
    internal static bool TryShowVolumeOsd()
    {
        object? shell = null;
        object? dispatcher = null;
        try
        {
            var shellClass = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
            var providerId = typeof(IShellServiceProvider).GUID;
            Marshal.ThrowExceptionForHR(
                CoCreateInstance(ref shellClass, 0, 4, ref providerId, out shell)
            );
            var dispatcherId = typeof(IFlyoutDispatcher).GUID;
            Marshal.ThrowExceptionForHR(
                ((IShellServiceProvider)shell).QueryService(
                    ref dispatcherId,
                    ref dispatcherId,
                    out dispatcher
                )
            );

            // This private shell interface shows the media-key OSD without injecting
            // keys or changing volume/mute. Reacquire it so Explorer restarts recover.
            Marshal.ThrowExceptionForHR(((IFlyoutDispatcher)dispatcher).ShowFlyout(0, 0));
            return true;
        }
        catch (Exception ex)
        {
            // Shell versions/sessions without this interface must not spam notifications.
            AudioDiagnostics.Log.Failure("show Windows volume OSD", ex);
            return false;
        }
        finally
        {
            if (dispatcher is not null)
            {
                try
                {
                    Marshal.ReleaseComObject(dispatcher);
                }
                catch (Exception ex)
                {
                    AudioDiagnostics.Log.Failure("release OSD dispatcher", ex);
                }
            }
            if (shell is not null)
            {
                try
                {
                    Marshal.ReleaseComObject(shell);
                }
                catch (Exception ex)
                {
                    AudioDiagnostics.Log.Failure("release OSD shell", ex);
                }
            }
        }
    }

    [DllImport("ole32.dll")]
    private static extern int CoCreateInstance(
        ref Guid classId,
        nint outer,
        uint context,
        ref Guid interfaceId,
        [MarshalAs(UnmanagedType.Interface)] out object instance
    );

    [ComImport]
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellServiceProvider
    {
        [PreserveSig]
        int QueryService(
            ref Guid service,
            ref Guid interfaceId,
            [MarshalAs(UnmanagedType.Interface)] out object instance
        );
    }

    [ComImport]
    [Guid("41F9D2FB-7834-4AB6-8B1B-73E74064B465")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFlyoutDispatcher
    {
        [PreserveSig]
        int ShowFlyout(int kind, uint flags);
    }
}
