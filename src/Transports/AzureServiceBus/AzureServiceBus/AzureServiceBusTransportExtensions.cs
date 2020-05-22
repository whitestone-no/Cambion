using System;
using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public static class AzureServiceBusTransportExtensions
    {
        public static ICambionConfiguration UseAzureServiceBus(this TransportConfiguration transportConfiguration, string endpoint)
        {
            AzureServiceBusConfig config = new AzureServiceBusConfig(endpoint);
            AzureServiceBusTransport transport = new AzureServiceBusTransport(config);
            return transportConfiguration.Transport(transport);
        }

        public static ICambionConfiguration UseAzureServiceBus(this TransportConfiguration transportConfiguration, string endpoint, Action<AzureServiceBusConfig> configAction)
        {
            AzureServiceBusConfig config = new AzureServiceBusConfig(endpoint);
            configAction(config);
            AzureServiceBusTransport transport = new AzureServiceBusTransport(config);
            return transportConfiguration.Transport(transport);
        }
    }
}
