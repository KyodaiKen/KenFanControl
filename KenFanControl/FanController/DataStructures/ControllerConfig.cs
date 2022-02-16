using KenFanControl.DriverHelpers;

namespace KenFanControl.DataStructures
{
    public class ControllerConfig : ControllerCommon
    {
        public ThermalSensor[]? ThermalSensors { get; set; }
        public PWMChannel[]? PWMChannels { get; set; }

        public void Deserialize(ControllerConfigDeserializationHelper data, DeviceCapabilities DeviceCapabilities)
        {
            if (data.RawResistors.Length != DeviceCapabilities.NumberOfSensors * 4)
            {
                throw new ArgumentException("Controller returned the wrong number of sensor information");
            }
            if (data.RawOffsets.Length != DeviceCapabilities.NumberOfSensors * 4)
            {
                throw new ArgumentException("Controller returned the wrong number of sensor information");
            }
            if (data.RawSteinhartHartCoefficients.Length != DeviceCapabilities.NumberOfSensors * 12)
            {
                throw new ArgumentException("Controller returned the wrong number of sensor information");
            }
            if (data.RawPins.Length != DeviceCapabilities.NumberOfSensors + DeviceCapabilities.NumberOfChannels)
            {
                throw new ArgumentException("Controller returned the wrong number of sensor information");
            }

            ThermalSensors = new ThermalSensor[DeviceCapabilities.NumberOfSensors];

            for (int i = 0; i < DeviceCapabilities.NumberOfSensors; i++)
            {
                ThermalSensors[i] = new ThermalSensor();

                var dataRange = (i * 4)..(i * 4 + 4);

                var unitData = new ThermalSensorsDeserializationHelper()
                {
                    RawOffset = data.RawOffsets[dataRange],
                    RawResistor = data.RawResistors[dataRange],
                    RawSteinhartHartCoefficients = data.RawSteinhartHartCoefficients[(i * 12)..((i + 1) * 12)],
                    RawPin = data.RawPins[i],
                };

                ThermalSensors[i].Deserialize(unitData);
            }

            PWMChannels = new PWMChannel[DeviceCapabilities.NumberOfChannels];

            for (int i = DeviceCapabilities.NumberOfSensors; i < DeviceCapabilities.NumberOfChannels; i++)
            {
                var channel = new PWMChannel()
                {
                    Pin = data.RawPins[i]
                };

                PWMChannels[i] = channel;
            }
        }

        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
