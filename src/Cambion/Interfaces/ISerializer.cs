using System.Threading.Tasks;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Interfaces
{
    public interface ISerializer
    {
        Task<byte[]> Serialize(MessageWrapper message);
        Task<MessageWrapper> Deserialize(byte[] serialized);
    }
}
