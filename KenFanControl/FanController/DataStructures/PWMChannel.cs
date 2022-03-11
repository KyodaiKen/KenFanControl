namespace KenFanControl.DataStructures
{
    public class PWMChannel : ControllerCommon
    {
        public byte Pin { get; set; }

        // Done that way to keep consistency
        public void Deserialize(byte raw)
        {
            Pin = raw;
        }

        public byte Serialize()
        {
            return Pin;
        }
    }
}
