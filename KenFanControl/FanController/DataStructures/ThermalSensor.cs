using KenFanControl.DriverHelpers;

namespace KenFanControl.DataStructures
{
    public class ThermalSensor : ControllerCommon
    {
        public float[]? CalibrationSteinhartHartCoefficients { get; set; }
        public float CalibrationOffset { get; set; }
        public float CalibrationResistorValue { get; set; }
        public byte Pin { get; set; }

        public void Deserialize(ThermalSensorsDeserializationHelper data)
        {
            CalibrationResistorValue = BitConverter.ToSingle(data.RawResistor);
            CalibrationOffset = BitConverter.ToSingle(data.RawOffset);

            CalibrationSteinhartHartCoefficients = new float[3];
            for (int c = 0; c < 3; c++)
            {
                int idc = c * 4;
                CalibrationSteinhartHartCoefficients[c] = BitConverter.ToSingle(data.RawSteinhartHartCoefficients[idc..(idc + 4)]);
            }

            Pin = data.RawPin;
        }

        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
