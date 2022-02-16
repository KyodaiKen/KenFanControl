namespace KenFanControl.DataStructures
{
    public class Matrix : ControllerCommon
    {
        public byte ChannelId { get; set; }
        public float[]? MatrixPoints { get; set; }

        public override void Deserialize(Span<byte> raw)
        {
            throw new NotImplementedException();
        }

        public override Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
