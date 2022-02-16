namespace KenFanControl.DataStructures
{
    public class Curve : ControllerCommon
    {
        public byte ChannelId { get; set; }
        public CurvePoint[]? CurvePoints { get; set; }

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
