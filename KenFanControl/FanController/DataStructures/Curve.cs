namespace KenFanControl.DataStructures
{
    public class Curve : ControllerCommon
    {
        public byte ChannelId { get; set; }
        public CurvePoint[]? CurvePoints { get; set; }

        public void Deserialize(Span<byte> raw, byte channelId)
        {
            // Convert data to curve
            byte len = raw[0];

            //Remove the length from so we only have the curve data.
            var data = raw[1..]; 

            CurvePoint[] cps = new CurvePoint[len];
            //Those offsets are headache to the power of 1000

            for (int i = 0; i < len; i++)
            {
                const int lenght = 5;
                int offset = i * lenght;

                cps[i] = new CurvePoint();

                cps[i].Deserialize(data[offset..(offset+lenght)]);
            }
            ChannelId = channelId;
            CurvePoints = cps;
        }

        public byte[] Serialize()
        {
            const int singleSize = 5;
            int dataSize = CurvePoints.Length * singleSize;

            // +1 to add the length byte
            byte[] payload = new byte[1 + dataSize];
            payload[0] = (byte)CurvePoints.Length;

            var offset = 1;
            
            for (int i = 0; i < CurvePoints.Length; i++)
            {
                var data = CurvePoints[i].Serialize();

                Array.Copy(data, 0, payload, offset, data.Length);
                offset += singleSize;
            }

            return payload;
        }
    }
}
