using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public static class AzureServiceBusTransportExtensions
    {
        public static ICambionSerializerBuilder UseAzureServiceBusTransport(this ICambionTransportBuilder builder, Action<AzureServiceBusConfig> configure)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ITransport), typeof(AzureServiceBusTransport), ServiceLifetime.Singleton));

            builder.Services.AddOptions<AzureServiceBusConfig>()
                .Configure(conf =>
                {
                    configure?.Invoke(conf);
                });

            return (ICambionSerializerBuilder)builder;
        }
    }
}
