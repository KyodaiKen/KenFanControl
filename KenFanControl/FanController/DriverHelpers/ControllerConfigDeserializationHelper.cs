namespace KenFanControl.DriverHelpers
{
    public ref struct ControllerConfigDeserializationHelper
    {
        public Span<byte> RawResistors { get; set; }
        public Span<byte> RawOffsets { get; set; }
        public Span<byte> RawSteinhartHartCoefficients { get; set; }
        public Span<byte> RawPins { get; set; }
    }

    public ref struct ThermalSensorsDeserializationHelper
    {
        public Span<byte> RawResistor { get; set; }
        public Span<byte> RawOffset { get; set; }
        public Span<byte> RawSteinhartHartCoefficients { get; set; }
        public byte RawPin { get; set; }
    }
}
