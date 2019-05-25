using Whitestone.Cambion.Common.Configurations;
using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion.Transport.Loopback
{
    public static class LoopbackTransportExtensions
    {
        public static ICambionConfiguration UseLoopback(this TransportConfiguration transportConfiguration)
        {
            LoopbackTransport transport = new LoopbackTransport();
            return transportConfiguration.Transport(transport);
        }
    }
}
