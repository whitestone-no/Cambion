using System;
using Microsoft.Extensions.DependencyInjection;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion
{
    public class CambionBuilder : ICambionSerializerBuilder, ICambionTransportBuilder
    {
        public IServiceCollection Services { get; }

        public CambionBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
