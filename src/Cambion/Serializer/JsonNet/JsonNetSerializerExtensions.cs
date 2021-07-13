using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Serializer.JsonNet
{
    public static class JsonNetSerializerExtensions
    {
        public static ICambionBuilder UseJsonNetSerializer(this ICambionBuilder builder)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ISerializer), typeof(JsonNetSerializer), ServiceLifetime.Singleton));

            return builder;
        }
    }
}
