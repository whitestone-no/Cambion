using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.NetMQ
{
    public static class NetMqTransportExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static ICambionSerializerBuilder UseNetMqTransport(this ICambionTransportBuilder builder, Action<NetMqConfig> configure)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ITransport), typeof(NetMqTransport), ServiceLifetime.Singleton));

            builder.Services.AddOptions<NetMqConfig>()
                .Configure(conf => { configure?.Invoke(conf); });

            return (ICambionSerializerBuilder)builder;
        }
    }
}