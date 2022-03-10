 using RJCP.IO.Ports;
using System.Runtime.Serialization.Formatters.Binary;

namespace KenFanControl
{
    // Investigate BinaryReader Class https://docs.microsoft.com/en-us/dotnet/api/system.io.binaryreader?view=net-6.0
    public static class Extensions
    {
        public static async Task SendCommand(this SerialPortStream SerialPort, params byte[] data)
        {
            await SerialPort.WriteAsync(data, 0, data.Length);
            //Fix reliability issues
            while(SerialPort.BytesToWrite > 0) await SerialPort.FlushAsync();
        }

        // Remove current data so the next time wait's for the updated data instead of reading the old cache
        public static bool TryGetAndRemove<T, Y>(this Dictionary<T, Y> Dictionary, T key, out Y data) where T : notnull
        {
            if (!Dictionary.ContainsKey(key))
            {
                data = default(Y);
                return false;
            }

            data = Dictionary[key];
            Dictionary.Remove(key);
            return true;
        }

        public static byte[] Concatenate(this byte[] first, byte[] second)
        {
            var final = new byte[first.Length + second.Length];

            Array.Copy(first, 0, final, 0, first.Length);
            Array.Copy(second, 0, final, first.Length, second.Length);

            return final;
        }

        public static byte[] Concatenate(this byte first, byte[] second)
        {
            var final = new byte[1 + second.Length];

            final[0] = first;
            Array.Copy(second, 0, final, 1, second.Length);

            return final;
        }

        public static byte[] Concatenate(this byte[] first, byte second)
        {
            var final = new byte[first.Length + 1];

            Array.Copy(first, 0, final, 0, first.Length);
            final[first.Length] = second;

            return final;
        }
    }
}
