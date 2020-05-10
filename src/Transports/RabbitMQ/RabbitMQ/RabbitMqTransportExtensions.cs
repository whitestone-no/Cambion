using System;
using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.RabbitMQ
{
    public static class RabbitMqTransportExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static ICambionConfiguration UseRabbitMQ(this TransportConfiguration transportConfiguration, string connectionString)
        {
            RabbitMqTransport transport = new RabbitMqTransport(connectionString);
            return transportConfiguration.Transport(transport);
        }

        // ReSharper disable once InconsistentNaming
        public static ICambionConfiguration UseRabbitMQ(this TransportConfiguration transportConfiguration, Action<RabbitMqConfig> configAction)
        {
            RabbitMqConfig config = new RabbitMqConfig();
            configAction(config);

            RabbitMqTransport transport = new RabbitMqTransport(config);
            return transportConfiguration.Transport(transport);
        }

    }
}
