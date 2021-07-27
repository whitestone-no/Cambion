using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Serializer.JsonNet
{
    public class JsonNetSerializer : ISerializer
    {
        public Task<MessageWrapper> Deserialize(byte[] messageBytes)
        {
            string json = Encoding.ASCII.GetString(messageBytes);
            MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            return Task.FromResult(wrapper);
        }

        public Task<byte[]> Serialize(MessageWrapper message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string json = JsonConvert.SerializeObject(message, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);

            return Task.FromResult(rawBytes);
        }
    }
}
