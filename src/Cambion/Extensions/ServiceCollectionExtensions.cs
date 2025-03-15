using Microsoft.Extensions.DependencyInjection;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static ICambionBuilder AddCambion(this IServiceCollection services)
        {
            ICambionBuilder builder = new CambionBuilder(services);

            services.AddSingleton<ICambion, Cambion>();

            services.AddHostedService(svc => svc.GetRequiredService<ICambion>());

            return builder;
        }
    }
}
