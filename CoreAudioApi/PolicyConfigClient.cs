using System.Runtime.InteropServices;
using AudioSwitch.CoreAudioApi.Interfaces;

namespace AudioSwitch.CoreAudioApi
{
    [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
    internal class _PolicyConfigClient
    {
    }

    public class PolicyConfigClient
    {
        private readonly IPolicyConfig _PolicyConfig = new _PolicyConfigClient() as IPolicyConfig;

        public void SetDefaultEndpoint(string devID, ERole eRole)
        {
            Marshal.ThrowExceptionForHR(_PolicyConfig.SetDefaultEndpoint(devID, eRole));
        }
    }
}