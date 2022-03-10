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

        public override Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
