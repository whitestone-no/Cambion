using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.NetMQ
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