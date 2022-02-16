namespace KenFanControl.DataStructures
{
    public class FanControlConfig : ControllerCommon
    {
        public Matrix[]? Matrixes { get; set; }
        public Curve[]? Curves { get; set; }

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
