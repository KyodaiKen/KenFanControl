namespace KenFanControl.DataStructures
{
    public class Readings : ControllerCommon
    {
        public TemperatureReading[]? TemperatureReadings { get; set; }
        public ChannelReading[]? ChannelReadings { get; set; }

        public void Deserialize(Span<byte> raw, DeviceCapabilities DeviceCapabilities)
        {
            // Convert data to curve

            //Check data length and validate it
            if (raw.Length != DeviceCapabilities.NumberOfSensors * 4 + DeviceCapabilities.NumberOfChannels * 8)
            {
                throw new ArgumentException("Data length does not match device capabilities.");
            }

            TemperatureReadings = new TemperatureReading[DeviceCapabilities.NumberOfSensors];

            for (int i = 0; i < TemperatureReadings.Length; i++)
            {
                int di = i * 4;

                var reading = new TemperatureReading();
                reading.Deserialize(raw[di..(di + 4)]);
                TemperatureReadings[i] = reading;
            }

            ChannelReadings = new ChannelReading[DeviceCapabilities.NumberOfChannels];

            for (int i = 0; i < TemperatureReadings.Length; i++)
            {
                int di = i * 8 + 12;

                var reading = new ChannelReading();

                reading.Deserialize(raw[di..(di+8)]);

                ChannelReadings[i] = reading;
            }
        }

        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
