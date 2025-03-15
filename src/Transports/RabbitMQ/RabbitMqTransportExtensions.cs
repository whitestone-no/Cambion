using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.RabbitMQ
{
    public static class RabbitMqTransportExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static ICambionSerializerBuilder UseRabbitMqTransport(this ICambionTransportBuilder builder, string connectionString)
        {
            return builder.UseRabbitMqTransport(conf => conf.Connection.ConnectionString = new Uri(connectionString));
        }

        // ReSharper disable once InconsistentNaming
        public static ICambionSerializerBuilder UseRabbitMqTransport(this ICambionTransportBuilder builder, Action<RabbitMqConfig> configure)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ITransport), typeof(RabbitMqTransport), ServiceLifetime.Singleton));

            builder.Services.AddOptions<RabbitMqConfig>()
                .Configure(conf => { configure?.Invoke(conf); });

            return (ICambionSerializerBuilder)builder;
        }

    }
}
