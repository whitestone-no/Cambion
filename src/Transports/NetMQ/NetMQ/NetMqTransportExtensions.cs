using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.NetMQ
{
    public static class NetMqTransportExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static ICambionBuilder UseNetMQ(this ICambionBuilder builder, Action<NetMqConfig> configure)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ITransport), typeof(NetMqTransport), ServiceLifetime.Singleton));

            builder.Services.AddOptions<NetMqConfig>()
                .Configure(conf =>
                {
                    if (configure != null)
                    {
                        configure(conf);
                    }
                });

            return builder;
        }
    }
}