using System.Diagnostics;

namespace KenFanControl.DataStructures
{
    public class DeviceCapabilities : ControllerCommon
    {
        public byte NumberOfSensors;
        public byte NumberOfChannels;

        public void Deserialize(Span<byte> raw)
        {
            NumberOfSensors = raw[0];
            NumberOfChannels = raw[1];
        }

#warning Delete?
        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
