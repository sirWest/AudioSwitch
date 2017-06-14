using AudioSwitch.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AudioSwitch.Classes.Settings;

namespace AudioSwitch.Classes
{
    internal class Device
    {
        internal MMDevice MMDevice { get; set; }
        internal EDataFlow DataFlow { get; set; }
    }
}
