using Whitestone.Cambion.Common.Types;

namespace Whitestone.Cambion.Common.Interfaces
{
    public interface ISerializer
    {
        byte[] Serialize(MessageWrapper messageBytes);
        MessageWrapper Deserialize(byte[] serialized);
    }
}
