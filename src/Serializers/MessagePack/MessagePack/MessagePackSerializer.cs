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
            byte[] messageBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                await MessagePack_Serializer.Typeless.SerializeAsync(ms, message).ConfigureAwait(false);
                messageBytes = ms.ToArray();
            }

            return messageBytes;
        }

        public async Task<MessageWrapper> Deserialize(byte[] serialized)
        {
            MessageWrapper message;
            using (MemoryStream ms = new MemoryStream(serialized))
            {
                message = await MessagePack_Serializer.Typeless.DeserializeAsync(ms).ConfigureAwait(false) as MessageWrapper;
            }

            return message;
        }
    }
}
