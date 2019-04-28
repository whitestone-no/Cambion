using Newtonsoft.Json;
using System.Text;
using Whitestone.Cambion.Common.Interfaces;
using Whitestone.Cambion.Common.Types;

namespace Whitestone.Cambion.Serializers.JsonNet
{
    public class JsonNetSerializer : ISerializer
    {
        public MessageWrapper Deserialize(byte[] messageBytes)
        {
            string json = Encoding.ASCII.GetString(messageBytes);
            MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            return wrapper;
        }

        public byte[] Serialize(MessageWrapper messageBytes)
        {
            string json = JsonConvert.SerializeObject(messageBytes, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);

            return rawBytes;
        }
    }
}
