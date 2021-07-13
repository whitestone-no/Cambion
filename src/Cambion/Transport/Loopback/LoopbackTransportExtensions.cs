using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.Loopback
{
    public static class LoopbackTransportExtensions
    {
        public static ICambionBuilder UseLoopbackTransport(this ICambionBuilder builder)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ITransport), typeof(LoopbackTransport), ServiceLifetime.Singleton));

            return builder;
        }
    }
}
