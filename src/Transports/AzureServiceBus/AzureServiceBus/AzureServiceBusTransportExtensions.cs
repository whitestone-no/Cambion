using System;
using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public static class AzureServiceBusTransportExtensions
    {
        public static ICambionConfiguration UseAzureServiceBus(this TransportConfiguration transportConfiguration, Action<AzureServiceBusConfig> configAction)
        {
            AzureServiceBusConfig config = new AzureServiceBusConfig();
            configAction(config);
            AzureServiceBusTransport transport = new AzureServiceBusTransport(config);
            return transportConfiguration.Transport(transport);
        }
    }
}
