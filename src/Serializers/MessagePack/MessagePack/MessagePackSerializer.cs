using System.IO;
using System.Threading.Tasks;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;

using MessagePack_Serializer = MessagePack.MessagePackSerializer;

namespace Whitestone.Cambion.Serializer.MessagePack
{
    public class MessagePackSerializer : ISerializer
    {
        public async Task<byte[]> Serialize(MessageWrapper message)
        {
            MemoryStream ms = new MemoryStream();
            await MessagePack_Serializer.Typeless.SerializeAsync(ms, message);

            return ms.ToArray();
        }

        public async Task<MessageWrapper> Deserialize(byte[] serialized)
        {
            MemoryStream ms = new MemoryStream(serialized);
            MessageWrapper message = await MessagePack_Serializer.Typeless.DeserializeAsync(ms) as MessageWrapper;

            return message;
        }
    }
}
