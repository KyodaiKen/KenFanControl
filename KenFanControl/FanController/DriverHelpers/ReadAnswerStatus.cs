using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenFanControl.DriverHelpers
{
    internal class ReadResults
    {
        public ReadStatus ReadStatus { get; set; }
        public byte ResponseKind { get; set; }
        public Memory<byte> Data { get; set; }
        public Span<byte> DataSpan { get => Data.Span; }
    }

    public enum ReadStatus
    {
        Ok,
        Warning,
        Error,
    }
}
