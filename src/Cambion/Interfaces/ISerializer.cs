using System.Threading.Tasks;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Interfaces
{
    public interface ISerializer
    {
        Task<byte[]> SerializeAsync(MessageWrapper message);
        Task<MessageWrapper> DeserializeAsync(byte[] serialized);
    }
}
