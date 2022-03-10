namespace KenFanControl.DataStructures
{
    public class Matrix : ControllerCommon
    {
        public byte ChannelId { get; set; }
        public float[]? MatrixPoints { get; set; }

        public void Deserialize(Span<byte> raw, byte channelId, DeviceCapabilities deviceCapabilities)
        {
            //Convert data to curve
            //Remove the length from so we only have the curve data.

            if (raw.Length / 4 != deviceCapabilities.NumberOfChannels)
                throw new InvalidDataException("Returned data lentgh has a different length than expected according to" +
                    "the number of channels on this controller.");

            MatrixPoints = new float[deviceCapabilities.NumberOfChannels];

            for (int i = 0; i < MatrixPoints.Length; i++)
            {
                int di = i * 4;
                MatrixPoints[i] = BitConverter.ToSingle(raw[di..(di + 4)]);
            }

            ChannelId = channelId;
        }

        public byte[] Serialize()
        {
            const int singleSize = 4;
            var dataSize = MatrixPoints.Length * singleSize;

            // + to add the channel id
            byte[] payload = new byte[1 + dataSize];
            payload[0] = ChannelId;

            var offset = 1;

            for (int i = 0; i < MatrixPoints.Length; i++)
            {
                var data = BitConverter.GetBytes(MatrixPoints[i]);
                Array.Copy(data, 0, payload, offset, data.Length);
                offset += singleSize;
            }

            return payload;
        }
    }
}
