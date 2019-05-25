using Whitestone.Cambion.Common.Configurations;
using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion.Transports.NetMQ
{
    public static class NetMqTransportExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static ICambionConfiguration UseNetMQ(this TransportConfiguration transportConfiguration, string publishAddress, string subscribeAddress, bool useMessageHost = false)
        {
            NetMqTransport transport = new NetMqTransport(publishAddress, subscribeAddress, useMessageHost);
            return transportConfiguration.Transport(transport);
        }
    }
}
