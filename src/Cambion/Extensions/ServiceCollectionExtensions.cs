using Microsoft.Extensions.DependencyInjection;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.Loopback;

namespace Whitestone.Cambion.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static ICambionBuilder AddCambion(this IServiceCollection services)
        {
            ICambionBuilder builder = new CambionBuilder(services);

            services.AddSingleton<ICambion, Cambion>();

            services.AddSingleton<ITransport, LoopbackTransport>();
            services.AddSingleton<ISerializer, JsonNetSerializer>();

            return builder;
        }
    }
}
