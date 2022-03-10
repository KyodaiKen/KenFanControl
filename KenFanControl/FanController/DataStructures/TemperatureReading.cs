namespace KenFanControl.DataStructures
{
    public class TemperatureReading : ControllerCommon
    {
        public float Temperature { get; set; }

        // Done that way to keep consistency
        public void Deserialize(Span<byte> raw)
        {
            Temperature = BitConverter.ToSingle(raw);
        }

#warning delete maybe?
        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
