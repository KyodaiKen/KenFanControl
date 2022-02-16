namespace KenFanControl.DataStructures
{
    public class ChannelReading : ControllerCommon
    {
        public float MatrixResult { get; set; }
        public float DutyCycle { get; set; }

        public void Deserialize(Span<byte> raw)
        {
            MatrixResult = BitConverter.ToSingle(raw[0..4]);
            DutyCycle = BitConverter.ToSingle(raw[4..8]);
        }

        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
