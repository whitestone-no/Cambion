using Newtonsoft.Json;
using System.Text;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Serializers.JsonNet
{
    public class Serializer : ISerializer
    {
        public MessageWrapper Deserialize(byte[] messageBytes)
        {
            string json = Encoding.ASCII.GetString(messageBytes);
            MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            return wrapper;
        }

        public byte[] Serialize(MessageWrapper message)
        {
            string json = JsonConvert.SerializeObject(message, Formatting.None,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);

            return rawBytes;
        }
    }
}
