namespace Whitestone.Cambion.Interfaces
{
    public interface ISerializer
    {
        byte[] Serialize(MessageWrapper messageBytes);
        MessageWrapper Deserialize(byte[] serialized);
    }
}
