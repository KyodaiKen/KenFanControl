//using System.Runtime.Serialization;

using System.Text;

namespace KenFanControl.DataStructures
{
    public abstract class ControllerCommon// : ISerializable
    {
        //void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    throw new NotImplementedException();
        //}

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);

            //var type = GetType();
            //var properties = type.GetProperties();

            //var builder = new StringBuilder();

            //for (int i = 0; i < properties.Length; i++)
            //{
            //    var property = properties[i];
            //    builder.AppendFormat("{0} => {1}", property.Name, property.GetValue(this));
            //    builder.AppendLine();
            //}

            //builder.AppendLine();
            //return builder.ToString();
        }

    }
}
