using KenFanControl.DriverHelpers;

namespace KenFanControl.DataStructures
{
    public class ThermalSensor : ControllerCommon
    {
        public const int SerializedSize = 4 * 5 + 1;
        // should be 3
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

        public byte[] Serialize()
        {
            const int floatSize = 4;

            // +1 for chanel, then resistor then *3 sh
            var payload = new byte[SerializedSize];
            payload[0] = Pin;

            var offset = 1;
            var res = BitConverter.GetBytes(CalibrationResistorValue);
            Array.Copy(res, 0, payload, offset, res.Length);
            offset += floatSize;

            var res1 = BitConverter.GetBytes(CalibrationOffset);
            Array.Copy(res1, 0, payload, offset, res1.Length);
            offset += floatSize;

            for (int c = 0; c < 3; c++)
            {
                var val = BitConverter.GetBytes(CalibrationSteinhartHartCoefficients[c]);
                Array.Copy(val, 0, payload, offset, val.Length);
                offset += floatSize;
            }

            return payload;
        }
    }
}
