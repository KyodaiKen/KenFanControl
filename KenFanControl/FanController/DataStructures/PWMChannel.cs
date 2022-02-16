namespace KenFanControl.DataStructures
{
    public class PWMChannel : ControllerCommon
    {
        public byte Pin { get; set; }

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
