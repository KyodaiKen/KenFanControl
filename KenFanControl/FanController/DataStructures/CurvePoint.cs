namespace KenFanControl.DataStructures
{
    public class CurvePoint : ControllerCommon
    {
        public float Temperature { get; set; }
        public byte DutyCycle { get; set; }

        public void Deserialize(Span<byte> raw)
        {
            Temperature = BitConverter.ToSingle(raw[0..3]);
            DutyCycle = raw[3..4][0];
        }

        public byte[] Serialize()
        {
            var temp = BitConverter.GetBytes(Temperature);
            var result = temp.Concatenate(DutyCycle);

            return result;
        }
    }
}
