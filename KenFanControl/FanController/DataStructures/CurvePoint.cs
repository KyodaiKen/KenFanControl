namespace KenFanControl.DataStructures
{
    public class CurvePoint : ControllerCommon
    {
        public float Temperature { get; set; }
        public byte DutyCycle { get; set; }

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
