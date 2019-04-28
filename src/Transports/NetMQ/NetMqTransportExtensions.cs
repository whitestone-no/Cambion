using Whitestone.Cambion.Common.Configurations;
using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion.Transports.NetMQ
{
    public static class NetMqTransportExtensions
    {
        public static ICambionConfiguration UseLoopback(this TransportConfiguration transportConfiguration)
        {
            NetMqTransport transport = new NetMqTransport();
            return transportConfiguration.Transport(transport);
        }
    }
}
