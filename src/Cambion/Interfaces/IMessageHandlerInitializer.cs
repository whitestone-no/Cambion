namespace Whitestone.Cambion.Interfaces
{
    public interface IMessageHandlerInitializer
    {
        IBackendTransport Transport { get; set; }
        ISerializer Serializer { get; set; }
    }
}
