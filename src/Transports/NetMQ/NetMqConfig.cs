using Whitestone.Cambion.Attributes;

namespace Whitestone.Cambion.Transport.NetMQ
{
    [CambionConfiguration]
    public class NetMqConfig
    {
        public string PublishAddress { get; set; }
        public string SubscribeAddress { get; set; }
        public bool UseMessageHost { get; set; }
    }
}
