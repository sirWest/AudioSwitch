namespace AudioSwitch.Core.Audio;

public sealed class AudioUnavailableException(string message) : InvalidOperationException(message)
{
    public static bool IsUnavailable(Exception exception) =>
        exception is AudioUnavailableException || IsUnavailable(exception.HResult);

    internal static bool IsUnavailable(int result) =>
        result
            is unchecked((int)0x80070490)
                or // E_NOTFOUND
                unchecked((int)0x8007048F)
                or // ERROR_DEVICE_NOT_CONNECTED
                unchecked((int)0x88890004)
                or // AUDCLNT_E_DEVICE_INVALIDATED
                unchecked((int)0x88890010)
                or // AUDCLNT_E_SERVICE_NOT_RUNNING
                unchecked((int)0x88890026); // AUDCLNT_E_RESOURCES_INVALIDATED
}
