namespace KenFanControl.DataStructures
{
    public class TemperatureReading : ControllerCommon
    {
        public float Temperature { get; set; }

        public void Deserialize(Span<byte> raw)
        {
            Temperature = BitConverter.ToSingle(raw);
        }

        public Memory<byte> Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
