namespace Whitestone.Cambion.Backend.NetMQ
{
    public interface INetMqConfigurator
    {
        string PublishAddress { get; set; }
        string SubscribeAddress { get; set; }
        void Start();
    }
}
