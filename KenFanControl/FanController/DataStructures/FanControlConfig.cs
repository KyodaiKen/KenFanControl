namespace KenFanControl.DataStructures
{
#warning is this used?
    public class FanControlConfig : ControllerCommon
    {
        public Matrix[]? Matrixes { get; set; }
        public Curve[]? Curves { get; set; }

#warning unused?
        public void Deserialize(Span<byte> raw)
        {
            throw new NotImplementedException();
        }

#warning unused?
        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
