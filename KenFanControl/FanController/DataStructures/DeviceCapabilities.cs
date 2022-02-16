using System.Diagnostics;

namespace KenFanControl.DataStructures
{
    public class DeviceCapabilities : ControllerCommon
    {
        public byte NumberOfSensors;
        public byte NumberOfChannels;

        public override void Deserialize(Span<byte> raw)
        {
            NumberOfSensors = raw[0];
            NumberOfChannels = raw[1];
        }

        public override Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
