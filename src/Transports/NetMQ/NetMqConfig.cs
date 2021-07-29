namespace Whitestone.Cambion.Transport.NetMQ
{
    public class NetMqConfig
    {
        public string PublishAddress { get; set; }
        public string SubscribeAddress { get; set; }
        public bool UseMessageHost { get; set; } = false;
    }
}
