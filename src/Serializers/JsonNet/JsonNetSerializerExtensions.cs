using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Serializer.JsonNet
{
    public static class JsonNetSerializerExtensions
    {
        public static void WithJsonNetSerializer(this ICambionSerializerBuilder builder)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ISerializer), typeof(JsonNetSerializer), ServiceLifetime.Singleton));
        }
    }
}
